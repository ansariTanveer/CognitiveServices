using CloudComputingProject.Models;
using CloudComputingProject.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CloudComputingProject.BusinessLayer
{
    public class FaceDetectionService
    {
        public async Task<List<FaceDetectionModel>> GetFaceAnalysis(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.FACE_API_KEY);
                string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                    "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                    "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

                string uri = Constants.FACE_URL + "?" + requestParameters;
                HttpResponseMessage response;
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);

                    string contentString = await response.Content.ReadAsStringAsync();
                    List<FaceDetectionModel> faceDetails = JsonConvert.DeserializeObject<List<FaceDetectionModel>>(contentString);
                    return faceDetails;
                }
            }
            catch (Exception ex)
            {
                return new List<FaceDetectionModel>();
            }
        }

        public byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}
