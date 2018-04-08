﻿using ImageSharp360.Imaging;
using ImageSharp360.Watermaking;
using ImageSharp360.Watermaking.Algorithm;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace ImageSharpApi.Controllers
{
    public class ApiResponse
    {
        [JsonProperty(PropertyName = "error")]
        public bool Error { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class ImageRequest
    {

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "base64")]
        public string Base64 { get; set; }
    }

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImageProcessing(ImageRequest requestImage360, ImageRequest requestWatermark, string factor)
        {
            if (string.IsNullOrWhiteSpace(factor))
            {
                return Json("No hay imagen 360", JsonRequestBehavior.AllowGet);
            }

            string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images");

            ApiResponse response = new ApiResponse
            {
                Error = true,
                Status = "failed",
                Message = "Información no presente"
            };

            HttpPostedFileBase image360 = null;
            HttpPostedFileBase watermark = null;
            //int factor = 1;

            if (ModelState.IsValid)
            {
                // TODO: Validar tipo de archivo.

                if (image360 != null && watermark != null)
                {
                    Bitmap btmp = new Bitmap(image360.InputStream);

                    if (btmp.Width < 7776 || btmp.Height > 3888)
                    {
                        response = new ApiResponse
                        {
                            Error = true,
                            Status = "failed",
                            Message = "No son correctas las dimenciones de la imagen."
                        };
                    }
                    else if (image360 != null && image360.ContentLength > 0)
                    {
                        try
                        {
                            // Imagen 360
                            string _360ImageName = Guid.NewGuid() + Path.GetExtension(image360.FileName);
                            string _360ImagePath = Path.Combine(sPath, _360ImageName);
                            image360.SaveAs(_360ImagePath);

                            // Watermark
                            string _watermarkName = Guid.NewGuid() + Path.GetExtension(watermark.FileName);
                            string _watermarkPath = Path.Combine(sPath, _watermarkName);
                            watermark.SaveAs(_watermarkPath);

                            // Procesamiento de la imagen
                            var _360Image = new Bitmap360(_360ImagePath);
                            var _watermark = new WatermarkBitmap(_watermarkPath);

                            Watermarking proceso = new Watermarking(_360Image, _watermark, new Factores(1),
                                TissotIndicatrix.TopIndicatrix,
                                TissotIndicatrix.BottomIndicatrix,
                                TissotIndicatrix.FirstIndicatrix,
                                TissotIndicatrix.SecondIndicatrix,
                                TissotIndicatrix.ThirdIndicatrix,
                                TissotIndicatrix.FourthIndicatrix,
                                TissotIndicatrix.FifthIndicatrix,
                                TissotIndicatrix.SixthIndicatrix,
                                TissotIndicatrix.SeventhIndicatrix,
                                TissotIndicatrix.EighthIndicatrix,
                                TissotIndicatrix.NinthIndicatrix,
                                TissotIndicatrix.TenthIndicatrix,
                                TissotIndicatrix.EleventhIndicatrix,
                                TissotIndicatrix.TwelfthIndicatrix);

                            proceso.Prepare();

                            string resultPath = Path.Combine(sPath, Guid.NewGuid().ToString() + Path.GetExtension(image360.FileName));
                            var result = proceso.Apply();
                            result.Save(resultPath, ImageFormat.Jpeg);

                            response = new ApiResponse
                            {
                                Error = false,
                                Status = "processed",
                                Message = resultPath
                            };
                        }
                        catch (Exception ex) { }
                    }
                }
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}