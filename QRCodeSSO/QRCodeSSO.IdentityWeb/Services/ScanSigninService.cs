using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCodeSSO.IdentityWeb.Data;
using QRCodeSSO.IdentityWeb.Models;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QRCodeSSO.IdentityWeb.Services
{
    public interface IScanSigninService
    {
        Task<ScanSigninRecord> CreateNewRecord(string returnUrl);

        ScanSigninRecord GetRecordByCode(string signInCode);

        Task SaveRecord(ScanSigninRecord record, OAuthUserInfo userInfo);

        ScanUserInfo GetSignInUserInfo(string signInCode);
    }

    public class ScanSigninService: IScanSigninService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ScanSigninService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<ScanSigninRecord> CreateNewRecord(string returnUrl)
        {
            ScanSigninRecord record = new ScanSigninRecord()
            {
                SignInCode = Guid.NewGuid().ToString().Replace("-", ""),
                ReturnUrl = returnUrl,
                ExpirationTime = DateTime.Now.AddMinutes(30).ToUniversalTime()
            };

            _dbContext.ScanSigninRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            return record;
        }

        public ScanSigninRecord GetRecordByCode(string signInCode)
        {
            var record = _dbContext.ScanSigninRecords.FirstOrDefault(p => p.SignInCode == signInCode);
            return record;
        }

        public async Task SaveRecord(ScanSigninRecord record, OAuthUserInfo userInfo)
        {
            var wechatUser = _dbContext.WeChatUsers.FirstOrDefault(p => p.OpenId == userInfo.openid);
            if(wechatUser == null)
            {
                var user = new ApplicationUser { UserName = userInfo.nickname, Email = "dummyemail@qq.com" };
                var createResult = await _userManager.CreateAsync(user, "P@ssw0rd");

                wechatUser = new WeChatUser()
                {
                    UserId = user.Id,
                    OpenId = userInfo.openid,
                    NickName=userInfo.nickname
                };
                _dbContext.WeChatUsers.Add(wechatUser);
            }

            record.SignInUserId = wechatUser.UserId;
            record.ScanOpenId = wechatUser.OpenId;
            record.SignInTime = DateTime.Now.ToUniversalTime();

            _dbContext.ScanSigninRecords.Update(record);
            await _dbContext.SaveChangesAsync();
        }

        public ScanUserInfo GetSignInUserInfo(string signInCode)
        {
            var record = _dbContext.ScanSigninRecords.AsNoTracking().FirstOrDefault(p => p.SignInCode == signInCode);
            if (record == null || !record.SignInTime.HasValue)
                return null;
            else
            {
                ScanUserInfo userInfo = new ScanUserInfo()
                {
                    UserId = record.SignInUserId,
                    OpenId = record.ScanOpenId,
                    ScanTime = record.SignInTime.Value,
                    NickName = _dbContext.WeChatUsers.FirstOrDefault(p => p.OpenId == record.ScanOpenId).NickName
                };

                return userInfo;
            }
        }
    }
}
