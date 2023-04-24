using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using ShopNetWork.Extensions;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;

namespace ShopNetWork.Controllers
{
    /// <summary>
    /// 文件上传控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [Route("api/[controller]")]
        [ApiController]
        public class UpLoadController : ControllerBase
        {

            private readonly ISqlSugarClient db;
            private readonly IConfiguration configuration;
            public UpLoadController(ISqlSugarClient db, IConfiguration configuration)
            {
                this.db = db;
                this.configuration = configuration;
            }


            [HttpPost("IUpIMG")]
            public string ICreatIMG()
            {
                var file = HttpContext.Request.Form.Files[0];
                if (file == null)
                {
                    return "没有文件";
                }
                string[] limitPicture = { ".JPG", "JPEG", "PNG", "BMP" };
                //获取文件名称
                string extname = Path.GetExtension(file.FileName);
                //判断文件格式
                if (!limitPicture.Contains(extname.ToUpper()))
                {
                    return "文件格式错误";
                }
                //防止文件命名重复
                string newfilename = $"{Guid.NewGuid().ToString()}-" + extname;
                //文件路径

                string filepath = Path.Combine(Directory.GetCurrentDirectory().ToString(), Path.Combine("wwwroot/Images", newfilename));
                using (FileStream fileStream = new FileStream(filepath, FileMode.CreateNew))
                {
                    file.CopyTo(fileStream);
                    fileStream.Flush();
                }
                string apiurl = configuration.GetValue<string>("ApiUrl");
                return "http://localhost:4003/wwwroot/Images/" + newfilename;
            }

            /// <summary>
            /// 单文件上传1 uploadFileForAsset(IFormFile file)
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            [HttpPost("uploadFileForAsset")]
            public IActionResult uploadFileForAsset(IFormFile file)
            {

                try
                {
                    //http://localhost:63280/images/006yt1omgy1gb3ckp21m9j31yt1ebqv7.jpg
                    // 服务器将要存储文件的路径
                    string path = "/学习资料/vs项目/CarManage/CarMana/wwwroot/Images/";
                    //var Folder = AppDomain.CurrentDomain.BaseDirectory + path; //项目默认路径
                    var Folder = "D:/✈" + path;
                    if (Directory.Exists(Folder) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(Folder);
                    }
                    StreamReader reader = new StreamReader(file.OpenReadStream());
                    String content = reader.ReadToEnd();
                    String name = file.FileName; // 获取文件名
                    String filename = Folder + name;
                    if (System.IO.File.Exists(filename))//判断时候有文件有就删除
                    {
                        System.IO.File.Delete(filename);
                    }
                    using (FileStream fs = System.IO.File.Create(filename))
                    {
                        // 复制文件
                        file.CopyTo(fs);
                        // 清空缓冲区数据
                        fs.Flush();
                    }

                    return new JsonResult(new { success = true, msg = "上传成功！", result = path + name });
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { success = false, msg = "上传失败！", result = ex.Message });
                }

            }

            /// <summary>
            /// 多文件上传1
            /// </summary>
            /// <param name="files"></param>
            /// <returns></returns>
            [HttpPost("ManyuploadFileForAsset")]
            public IActionResult uploadFileForAsset(List<IFormFile> files)//这里有一个注意事项files的名称要和前端对应上///////////////////////
            {
                try
                {
                    // 服务器将要存储文件的路径
                    string path = Directory.GetCurrentDirectory() + "/wwwroot/Images/";
                    if (Directory.Exists(path) == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(path);
                    }
                    List<string> list = new List<string>();
                    foreach (var file in files)
                    {

                        String name = file.FileName; // 获取文件名
                        String filename = path + name;
                        string name2 = "http://localhost:4003/Image/" + name;
                        list.Add(name2);
                        if (System.IO.File.Exists(filename))
                        {
                            System.IO.File.Delete(filename);
                        }
                        using (FileStream fs = System.IO.File.Create(filename))
                        {
                            // 复制文件
                            file.CopyTo(fs);
                            // 清空缓冲区数据
                            fs.Flush();
                        }
                    }
                    return new JsonResult(new { success = true, msg = "上传成功！", result = list });
                }
                catch (Exception ex)
                {
                    return new JsonResult(new { success = false, msg = "上传失败！", result = ex.Message });
                }

            }



            /// <summary>
            /// 单文件上传3 var file = HttpContext.Request.Form.Files[0];
            /// </summary>
            /// <returns></returns>
            [HttpPost("UpFile")]
            public IActionResult UpFile()
            {
                //获取上传文件
                //HttpContext.Request包含请求的所有内容(包括文件),Form是表单,Files是文件
                var file = HttpContext.Request.Form.Files[0];

                //1文件上传到哪里(路径)
                // \\第一个斜杠是转译符
                string path = Directory.GetCurrentDirectory() + "/wwwroot/Images/" + file.FileName;//获取根目录

                //2文件上传
                //文件流

                using (FileStream fs = new FileStream(path, FileMode.Create))//为什么自动释放因为继承了一个接口IDispoable可以用using 
                {
                    //本质是将文件放到文件流里
                    file.CopyTo(fs);
                    //清楚缓存清理文件流
                    fs.Flush();
                }
                //返回图片完整路径 端口号 完整域名

                string filePath = "http://localhost:4003/Images/" + file.FileName;
                return Ok(filePath);

            }
            /// <summary>
            /// 多文件上传
            /// </summary>
            /// <returns></returns>
            [HttpPost("UpFiles")]
            public IActionResult UpFiles()
            {
                //获取上传文件
                //HttpContext.Request包含请求的所有内容(包括文件),Form是表单,Files是文件
                var files = HttpContext.Request.Form.Files;//这里有一个注意事项files的名称要和前端对应上///////////////////////
                List<string> fileNames = new List<string>();
                foreach (var file in files)
                {


                    //1文件上传到哪里(路径)
                    // \\第一个斜杠是转译符
                    string path = Directory.GetCurrentDirectory() + "/wwwroot/Images/" + file.FileName;//获取根目录

                    //2文件上传
                    //文件流

                    using (FileStream fs = new FileStream(path, FileMode.Create))//为什么自动释放因为继承了一个接口IDispoable可以用using 
                    {
                        //本质是将文件放到文件流里
                        file.CopyTo(fs);
                        //清楚缓存清理文件流
                        fs.Flush();
                    }
                    //返回图片完整路径 端口号 完整域名

                    string filePath = "http://localhost:63280/Images/" + file.FileName;
                    fileNames.Add(filePath);
                }
                return Ok(fileNames);

            }
            /// <summary>
            /// 切片上传
            /// </summary>
            /// <returns></returns>
            [HttpPost("ManyUploadFile")]
            public async Task<IActionResult> UploadFile()
            {
                var data = Request.Form.Files["data"];
                string lastModified = Request.Form["lastModified"].ToString();
                var total = Request.Form["total"];
                var fileName = Request.Form["fileName"];
                var index = Request.Form["index"];

                string temporary = Path.Combine($"{Directory.GetCurrentDirectory()}/wwwroot/", lastModified);//临时保存分块的目录
                try
                {
                    if (!Directory.Exists(temporary))
                        Directory.CreateDirectory(temporary);
                    string filePath = Path.Combine(temporary, index.ToString());
                    if (!Convert.IsDBNull(data))
                    {
                        await Task.Run(() => {
                            FileStream fs = new FileStream(filePath, FileMode.Create);
                            data.CopyTo(fs);
                        });
                    }
                    bool mergeOk = false;
                    if (total == index)
                    {
                        mergeOk = await FileMerge(lastModified, fileName);
                    }

                    Dictionary<string, object> result = new Dictionary<string, object>();
                    result.Add("number", index);
                    result.Add("mergeOk", mergeOk);
                    return Ok(result);

                }
                catch (Exception ex)
                {
                    Directory.Delete(temporary);//删除文件夹
                    throw ex;
                }
            }
            /// <summary>
            /// 合并切片
            /// </summary>
            /// <param name="lastModified"></param>
            /// <param name="fileName"></param>
            /// <returns></returns>
            [HttpGet("FileMerge")]
            public async Task<bool> FileMerge(string lastModified, string fileName)
            {
                bool ok = false;
                try
                {
                    var temporary = Path.Combine($"{Directory.GetCurrentDirectory()}/wwwroot/", lastModified);//临时文件夹
                    fileName = Request.Form["fileName"];//文件名
                    string fileExt = Path.GetExtension(fileName);//获取文件后缀
                    var files = Directory.GetFiles(temporary);//获得下面的所有文件
                    var finalPath = Path.Combine($"{Directory.GetCurrentDirectory()}/wwwroot/", DateTime.Now.ToString("yyMMddHHmmss") + fileExt);//最终的文件名（demo中保存的是它上传时候的文件名，实际操作肯定不能这样）
                    var fs = new FileStream(finalPath, FileMode.Create);
                    foreach (var part in files.OrderBy(x => x.Length).ThenBy(x => x))//排一下序，保证从0-N Write
                    {
                        var bytes = System.IO.File.ReadAllBytes(part);
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                        bytes = null;
                        System.IO.File.Delete(part);//删除分块
                    }
                    fs.Close();
                    Directory.Delete(temporary);//删除文件夹
                    ok = true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return ok;

            }






        }
    }
}
