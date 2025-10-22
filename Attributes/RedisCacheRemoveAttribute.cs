using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using EIU.Caching.Redis.Core;

namespace EIU.Caching.Redis.Attributes
{
    /// <summary>
    /// Attribute dùng để xóa cache Redis khi dữ liệu thay đổi (POST / PUT / DELETE).
    ///
    /// Hỗ trợ 3 chế độ:
    /// 1️⃣ Có truyền key cụ thể → xóa chính xác key hoặc prefix.
    ///     [RedisCacheRemove("Student:Detail:{id}", "Student:GetList")]
    ///
    /// 2️⃣ Không truyền gì nhưng Action có param id hoặc ...Id →
    ///     Tự động xóa cache detail + list tương ứng.
    ///     [RedisCacheRemove] với Delete(int id)
    ///     → Xóa Project:Controller:Detail:{id} và Project:Controller:GetList
    ///
    /// 3️⃣ Không có id → xóa toàn bộ cache của controller.
    ///     [RedisCacheRemove] với ImportAll()
    ///     → Xóa Project:Controller:*
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class RedisCacheRemoveAttribute : Attribute, IAsyncActionFilter
    {
        /// <summary>
        /// Danh sách key hoặc prefix chỉ định để xóa thủ công.
        /// </summary>
        public string[] KeysOrPrefixes { get; }

        public RedisCacheRemoveAttribute(params string[] keysOrPrefixes)
        {
            KeysOrPrefixes = keysOrPrefixes ?? Array.Empty<string>();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // ✅ Chạy action trước, chỉ xóa cache khi action thành công
            var result = await next();

            if (result.Exception != null)
                return; // ❌ Nếu có lỗi → không xóa cache

            var sp = context.HttpContext.RequestServices;
            var cacheService = sp.GetRequiredService<IRedisCacheService>();
            var options = sp.GetRequiredService<IOptions<RedisCacheOptions>>().Value;

            // ⚙️ Nếu Redis bị tắt trong cấu hình → bỏ qua
            if (!options.Enabled)
                return;

            var project = options.ProjectAlias ?? "default";
            var controller = context.Controller.GetType().Name.Replace("Controller", "");

            // ------------------------------------------------------------------------------------
            // 🧩 CASE 1: Có key hoặc prefix chỉ định → xóa chính xác theo danh sách
            // ------------------------------------------------------------------------------------
            if (KeysOrPrefixes.Length > 0)
            {
                await RemoveExplicitKeysAsync(KeysOrPrefixes, project, context, cacheService);
                return;
            }

            // ------------------------------------------------------------------------------------
            // 🧩 CASE 2: Không chỉ định key → tự phát hiện id hoặc ...Id trong tham số
            // ------------------------------------------------------------------------------------
            var idParam = context.ActionArguments.FirstOrDefault(p =>
                p.Key.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                p.Key.EndsWith("Id", StringComparison.OrdinalIgnoreCase)
            );

            if (idParam.Value != null)
            {
                // Có id → xóa chi tiết và danh sách liên quan
                var idValue = idParam.Value.ToString();
                var detailKey = $"{project}:{controller}:detail:{idValue}".ToLowerInvariant();
                var listKey = $"{project}:{controller}:getlist".ToLowerInvariant();

                await cacheService.RemoveAsync(detailKey);
                await cacheService.RemoveAsync(listKey);
            }
            else
            {
                // --------------------------------------------------------------------------------
                // 🧹 CASE 3: Không có id → xóa toàn bộ cache controller
                // --------------------------------------------------------------------------------
                var prefix = $"{project}:{controller}:".ToLowerInvariant();
                await cacheService.RemoveByPrefixAsync(prefix);
            }
        }

        /// <summary>
        /// Xử lý xóa cache khi truyền key cụ thể (có thể chứa {id} hoặc prefix)
        /// </summary>
        private async Task RemoveExplicitKeysAsync(
            string[] keys,
            string project,
            ActionExecutingContext context,
            IRedisCacheService cacheService)
        {
            foreach (var rawKey in keys)
            {
                // Thay placeholder {id} hoặc {userId} trong key template
                var key = ReplaceRouteValues(rawKey, context);
                var fullKey = $"{project}:{key}".ToLowerInvariant();

                if (fullKey.EndsWith(":"))
                {
                    // Xóa prefix
                    await cacheService.RemoveByPrefixAsync(fullKey);
                }
                else
                {
                    // Xóa key cụ thể
                    await cacheService.RemoveAsync(fullKey);
                }
            }
        }

        /// <summary>
        /// Thay {id}, {userId}, ... bằng giá trị thực tế trong route hoặc param.
        /// </summary>
        private string ReplaceRouteValues(string key, ActionExecutingContext context)
        {
            if (string.IsNullOrWhiteSpace(key))
                return key;

            foreach (var arg in context.ActionArguments)
            {
                if (arg.Value == null) continue;

                key = key.Replace(
                    $"{{{arg.Key}}}",
                    arg.Value.ToString(),
                    StringComparison.OrdinalIgnoreCase
                );
            }

            return key;
        }
    }
}
