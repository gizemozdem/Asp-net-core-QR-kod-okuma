using DatamatrixReader.Models;
using DatamatrixReader.Services;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using ZXing;
using ZXing.Common;

namespace DatamatrixReader.Controllers
{
    public class FileUploadController : Controller
    {
        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUploadModel model)
        {
            if (model.UploadedFiles != null && model.UploadedFiles.Count > 0)
            {
                var results = new List<string>();
                foreach (var file in model.UploadedFiles)
                {
                    if (file.ContentType == "image/jpeg" || file.ContentType == "image/png" || file.ContentType == "image/jpg")
                    {
                        if (file.Length > 0)
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);

                          
                                using (var bitmap = new Bitmap(stream))
                                {
                                    // Bitmap'i LuminanceSource'a dönüştürüldü
                                    var source = new BitmapLuminanceSource(bitmap);

                                    var reader = new BarcodeReaderGeneric
                                    {
                                        AutoRotate = true,
                                        TryInverted = true,
                                        Options = new DecodingOptions
                                        {
                                            TryHarder = true
                                        }
                                    };

                                    var result = reader.Decode(source);

                                    if (result != null)
                                    {
                                        results.Add($"File: {file.FileName} - Barcode: {result.Text}");
                                    }
                                    else
                                    {
                                        results.Add($"File: {file.FileName} - No barcode detected.");
                                    }
                                }
                            }
                        }
                    }
                }

                // Barkod sonuçlarını bir .txt dosyasına yazdırma işlemi.
                var txtFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", "BarcodeResults.txt");
                await System.IO.File.WriteAllLinesAsync(txtFilePath, results);

                // Dosyayı indirmek için kullanıcıya dosyayı döndürmek..
                var memory = new MemoryStream();
                using (var stream = new FileStream(txtFilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;
                return File(memory, "text/plain", "BarcodeResults.txt");
            }
            else
            {
                ViewBag.Message = "Please select at least one file!";
                return View();
            }
        }
    }
}
