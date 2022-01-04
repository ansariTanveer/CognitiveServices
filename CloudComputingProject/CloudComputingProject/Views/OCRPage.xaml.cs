using Acr.UserDialogs;
using CloudComputingProject.Utilities;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CloudComputingProject.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OCRPage : ContentPage
    {
        public OCRPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null)
                    return;

                ocrImage.Source = ImageSource.FromStream(() =>
                {
                    var stream = file.GetStream();
                    return stream;
                });


                IList<VisualFeatureTypes> features = new List<VisualFeatureTypes> {
                    VisualFeatureTypes.Categories,
                    VisualFeatureTypes.Description,
                    VisualFeatureTypes.Faces,
                    VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Tags
                };

                using (var progress = UserDialogs.Instance.Progress("Progress", null, null, true, MaskType.Black))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        progress.PercentComplete = i;
                        await Task.Delay(60);
                    }


                    var credentials = new ApiKeyServiceClientCredentials(Constants.OCR_API_KEY);
                    var handler = new System.Net.Http.DelegatingHandler[] { };

                    using (var visionClient = new ComputerVisionClient(credentials, handler))
                    {
                        try
                        {
                            var imageStream = file.GetStream();
                            imageStream.Position = 0;
                            visionClient.Endpoint = Constants.OCR_URL;
                            
                            progress.PercentComplete = 30;

                            var textHeaders = await visionClient.ReadInStreamAsync(imageStream);
                            string operationLocation = textHeaders.OperationLocation;
                            Thread.Sleep(2000);

                            const int numberOfCharsInOperationId = 36;
                            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

                            ReadOperationResult results;

                            do
                            {
                                results = await visionClient.GetReadResultAsync(Guid.Parse(operationId));
                            }
                            while ((results.Status == OperationStatusCodes.Running ||
                            results.Status == OperationStatusCodes.NotStarted));

                            var textUrlFileResults = results.AnalyzeResult.ReadResults;
                            ocrText.Text = string.Empty;
                            int count = 0;
                            foreach (ReadResult page in textUrlFileResults)
                            {
                                count = page.Lines.Count;
                                var progressPerc = 100 / 40;
                                foreach (Line line in page.Lines)
                                {
                                    ocrText.Text += line.Text + " ";
                                    progress.PercentComplete += progressPerc;
                                }
                            }
                            progress.PercentComplete = 100;
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}