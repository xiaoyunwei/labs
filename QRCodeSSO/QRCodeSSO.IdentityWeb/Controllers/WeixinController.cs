using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities.Request;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using Senparc.Weixin.MP.AdvancedAPIs;
using System.Web;
using Microsoft.AspNetCore.Http;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Senparc.Weixin.MP.AdvancedAPIs.QrCode;
using QRCoder;
using System.DrawingCore;
using System.DrawingCore.Imaging;
using Microsoft.AspNetCore.Identity;
using QRCodeSSO.IdentityWeb.Models;
using QRCodeSSO.IdentityWeb.Services;

namespace QRCodeSSO.IdentityWeb.Controllers
{
    public class WeixinController : Controller
    {
        private readonly WeixinSettings _weixinSettings;
        private readonly IScanSigninService _scanSigninService;

        const string OAUTH_STATE_SESSION = "OAUTH_STATE";

        public WeixinController(IOptions<WeixinSettings> optionsAccessor, IScanSigninService scanSigninService)
        {
            _weixinSettings = optionsAccessor.Value;
            _scanSigninService = scanSigninService;
        }

        public IActionResult Index(PostModel postModel, string echostr)
        {
            if (CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, _weixinSettings.Token))
            {
                return Content(echostr); //返回随机字符串则表示验证通过
            }
            else
            {
                return Content("failed:" + postModel.Signature + "," + CheckSignature.GetSignature(postModel.Timestamp, postModel.Nonce, _weixinSettings.Token)
                    + "。" + "如果你在浏览器中看到这句话，说明此地址可以被作为微信公众账号后台的Url，请注意保持Token一致。");
            }
        }

        [HttpPost]
        public IActionResult Index(PostModel postModel)
        {
            if (!CheckSignature.Check(postModel.Signature, postModel.Timestamp, postModel.Nonce, _weixinSettings.Token))
            {
                return Content("参数错误");
            }

            // 这里必须创建 MemoryStream 然后给 MessageHandler 处理
            // 直接将 Request.Body 传递给 MessageHandler 会抛异常
            MemoryStream ms = new MemoryStream();
            Request.Body.CopyToAsync(ms);
            var messageHandler = new WeixinMessageHandler(ms, postModel);
            messageHandler.Execute();

            if (messageHandler.FinalResponseDocument != null)
            {
                // 保存 Response文件，供调试用
                messageHandler.FinalResponseDocument.Save($"resp/{DateTime.Now.ToString("yyyyMMdd_hhmmss")}");
                
                return Content(messageHandler.TextResponseMessage, "text/xml", Encoding.UTF8);
            }
            else
            {
                return Content("");
            }
        }

        /// <summary>
        /// OAuthScope.snsapi_userinfo方式回调
        /// </summary>
        /// <param name="code"></param>
        /// <param name="state"></param>
        /// <param name="returnUrl">用户最初尝试进入的页面</param>
        /// <returns></returns>
        public async Task<IActionResult> UserInfoCallback(string code, string state, string signInCode)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Content("您拒绝了授权！");
            }

            OAuthAccessTokenResult result = null;

            //通过，用code换取access_token
            try
            {
                result = OAuthApi.GetAccessToken(_weixinSettings.AppId, _weixinSettings.AppSecret, code);

                if (result.errcode != ReturnCode.请求成功)
                {
                    return Content("错误：" + result.errmsg);
                }

                // 进一步获取用户详细信息
                OAuthUserInfo userInfo = OAuthApi.GetUserInfo(result.access_token, result.openid);

                // 登录用户
                var record = _scanSigninService.GetRecordByCode(signInCode);
                if (record == null)
                {
                    return Content("无效二维码");
                }
                else if(record.ExpirationTime < DateTime.Now.ToUniversalTime())
                {
                    return Content("二维码已过期");
                }
                else
                {
                    await _scanSigninService.SaveRecord(record, userInfo);
                    return Content("您已完成扫描登录");
                }
            }
            catch (Exception ex)
            {
                return Content($"{ex.Message}/r/n{ex.Source}/r/n{ex.StackTrace}");
            }
        }

        public async Task<IActionResult> GetSignInQrcode(string returnUrl)
        {
            var record = await _scanSigninService.CreateNewRecord(returnUrl);

            string url = $"{_weixinSettings.Url}/QrcodeSignIn?signInCode={record.SignInCode}";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            MemoryStream ms = new MemoryStream();
            qrCodeImage.Save(ms, ImageFormat.Jpeg);

            return File(ms.ToArray(), "image/jpeg");
        }

        public IActionResult QrcodeSignIn(string signInCode)
        {
            var record = _scanSigninService.GetRecordByCode(signInCode);
            if (record == null)
            {
                return Content("无效的二维码");
            }
            else if(record.ExpirationTime < DateTime.Now.ToUniversalTime())
            {
                return Content("二维码已过期");
            }
            else
            {
                string state = signInCode;

                string callBackUrl = $"{_weixinSettings.Url}/UserInfoCallback?signInCode={signInCode}";
                string authUrl = OAuthApi.GetAuthorizeUrl(_weixinSettings.AppId, callBackUrl, state, OAuthScope.snsapi_userinfo);

                return Redirect(authUrl);
            }
        }

        public async Task<ScanUserInfo> GetScanUser(string signInCode)
        {
            var record = _scanSigninService.GetRecordByCode(signInCode);
            if (record == null)
            {
                return null;
            }
            else
            {
                while (true)
                {
                    if (record.ExpirationTime < DateTime.Now.ToUniversalTime())
                    {
                        // 用户没有在有效期内扫描，已超过有效期
                        return null;
                    }
                    else
                    {
                        var userInfo = _scanSigninService.GetSignInUserInfo(signInCode);
                        if (userInfo == null)
                            await Task.Delay(2000);
                        else
                            return userInfo;
                    }
                }
            }
        }
    }
}