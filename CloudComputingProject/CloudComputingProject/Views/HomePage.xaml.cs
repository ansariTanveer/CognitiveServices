using Acr.UserDialogs;
using CloudComputingProject.BusinessLayer;
using CloudComputingProject.Models;
using Plugin.Media;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CloudComputingProject.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage
    {
        public HomePage()
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
                using (var progress = UserDialogs.Instance.Progress("Progress", null, null, true, MaskType.Black))
                {
                    imageSelected.Source = ImageSource.FromStream(() =>
                    {
                        var stream = file.GetStream();
                        return stream;
                    });
                    for (int i = 0; i < 20; i++)
                    {
                        progress.PercentComplete = i;
                        await Task.Delay(60);
                    }
                    var faceDetails = await new FaceDetectionService().GetFaceAnalysis(file.Path);

                    for (int i = 21; i < 100; i+=5)
                    {
                        progress.PercentComplete = i;
                        await Task.Delay(30);
                    }

                    if (faceDetails.Count != 0)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            totalFaceLbl.Text = faceDetails.Count.ToString();
                            ageLbl.Text = faceDetails[0].faceAttributes.age.ToString();
                            genderLbl.Text = faceDetails[0].faceAttributes.gender;
                            smileLbl.Text = GetSmileDetails(faceDetails[0].faceAttributes.smile);
                            facialHairLbl.Text = GetFacialHair(faceDetails[0].faceAttributes.facialHair);
                            glassLbl.Text = faceDetails[0].faceAttributes.glasses;
                            hairLbl.Text = GetHairDetails(faceDetails[0].faceAttributes.hair);
                            emotionLbl.Text = GetEmotions(faceDetails[0].faceAttributes.emotion);
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string GetSmileDetails(double smileValue)
        {
            if (smileValue == 0)
            {
                return "No Smile";
            }
            else if (smileValue > 0 && smileValue < 0.4)
            {
                return "Descent Smile";
            }
            else if (smileValue > 0.4 && smileValue < 0.8)
            {
                return "Laughing";
            }
            else
            {
                return "Loudly Laughing";
            }
        }

        private string GetFacialHair(FacialHair facialHair)
        {
            string facialHairText = "";
            if (facialHair.moustache == 0)
            {
                facialHairText += "No Moustache";
            }
            else if (facialHair.moustache > 0 && facialHair.moustache < 0.5)
            {
                facialHairText += "Medium Moustache";
            }
            else
            {
                facialHairText += "Dense Moustache";
            }

            if (facialHair.beard == 0)
            {
                facialHairText += ", No Beard";
            }
            else if (facialHair.beard > 0 && facialHair.beard < 0.5)
            {
                facialHairText += ", Medium Beard";
            }
            else
            {
                facialHairText += ", Dense Beard";
            }

            return facialHairText;
        }

        private string GetHairDetails(Hair hair)
        {
            string hairDetailsText = "";
            if (hair.invisible)
            {
                hairDetailsText = "Hair not visible";
                return hairDetailsText;
            }

            if (hair.bald == 0)
            {
                hairDetailsText = "Bald";
                return hairDetailsText;
            }

            HairColor haircolor = hair.hairColor.Where(x => x.confidence == hair.hairColor.Max(y => y.confidence)).FirstOrDefault();
            hairDetailsText = haircolor.color;
            return hairDetailsText;
        }

        private string GetEmotions(Emotion emotion)
        {
            int count = 0;
            double[] arr = new double[8];
            string[] propName = new string[8];
            PropertyInfo[] properties = typeof(Emotion).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                arr[count] = Convert.ToDouble(property.GetValue(emotion));
                propName[count] = property.Name;
                count++;
            }
            int maxIndex = arr.ToList().IndexOf(arr.Max());
            return propName[maxIndex];
        }
    }
}