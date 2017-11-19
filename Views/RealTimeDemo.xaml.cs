// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using IntelligentKioskSample.Controls;
using IntelligentKioskSample.Helper;
using Microsoft.ProjectOxford.Common;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace IntelligentKioskSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    //[KioskExperience(Title = "People Detection", ImagePath = "ms-appx:/Assets/realtime.png", ExperienceType = ExperienceType.Kiosk)]
    //public sealed partial class RealTimeDemo : Page, IRealTimeDataProvider
    public partial class RealTimeDemo : Page, IRealTimeDataProvider
    {
        private Task processingLoopTask;
        private bool isProcessingLoopInProgress;
        private bool isProcessingPhoto;

        private bool isSubmittingPictures;
        public bool ishammer = false;

        public Guid persionID = new Guid();
        public string groupid = "";

        private IEnumerable<Emotion> lastEmotionSample;
        private IEnumerable<Face> lastDetectedFaceSample;
        private IEnumerable<Tuple<Face, IdentifiedPerson>> lastIdentifiedPersonSample;
        private IEnumerable<SimilarFaceMatch> lastSimilarPersistedFaceSample;
        private int listItems;
        private int weblistItems;
        private DemographicsData demographics;
        private Dictionary<Guid, Visitor> visitors = new Dictionary<Guid, Visitor>();
        private int noofImages = 0;
        private int LastPersistedCount;
        private DateTime LastUpdatedDateTime = DateTime.Now;


        public string SessionID;


        #region popups
        bool ishelmet = false;
        bool unauth = false;
        bool unauthSuccess = false;
        bool unauthreport = false;
        bool UserMessage = false;
        bool UserMessageReply = false;
        bool firemess = false;

        #endregion
        bool isfiredetected = true;
        public RealTimeDemo()
        {
            this.InitializeComponent();

            this.DataContext = this;

            Window.Current.Activated += CurrentWindowActivationStateChanged;
            this.cameraControl.SetRealTimeDataProvider(this);
            this.cameraControl.FilterOutSmallFaces = true;
            this.cameraControl.HideCameraControls();
            this.cameraControl.CameraAspectRatioChanged += CameraControl_CameraAspectRatioChanged;
            UnauthorizedImagesGV.ItemsSource = imagelist;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += LoadingChats;
            timer.Start();

            getPredefinedStuff();

            Constants.isHelmet = false;




        }

        private async void getPredefinedStuff()
        {
            try
            {
                IEnumerable<PersonGroup> personGroups = await FaceServiceHelper.ListPersonGroupsAsync(SettingsHelper.Instance.WorkspaceKey);

                Person[] personsInGroup = await FaceServiceHelper.GetPersonsAsync(personGroups.FirstOrDefault().PersonGroupId);

                CreatePersonResult result = await FaceServiceHelper.CreatePersonAsync(personGroups.FirstOrDefault().PersonGroupId, "Rami");
                groupid = personGroups.FirstOrDefault().PersonGroupId;
                persionID = result.PersonId;

                Constants.x = "1920";
                Constants.y = "1350";
            }
            catch (Exception ex)
            {

            }

        }

        private async void GetMaterials()
        {
            try
            {
                var data = await getData("http://gitex.eastus.cloudapp.azure.com/gitexbackend/api/values/checkrect");
                var rectangledata = JsonConvert.DeserializeObject<List<ROIData>>(data);

                Constants.materials = new List<ROIData>();
                Constants.materials = rectangledata;
            }
            catch (Exception)
            {

            }

        }

        private async Task<string> getData(string url)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return await client.GetStringAsync(url);

        }
        bool islastUser = false;
        string answertext = "";
        private async void LoadingChats(object sender, object e)
        {
            try
            {
                //Calling the webservices 
                CurrentTime.Text = DateTime.Now.ToString(@"HH:mm:ss");



                await Task.Run(() => GetMaterials());

                await Task.Run(() => getNewChats());

                if (messages != null && messages.Count > 0)
                {
                    ChatUserControl control = new ChatUserControl();
                    control.setControlProperties(350, "USER", messages.FirstOrDefault().Question, "green", null, false);
                    ChatStack.Children.Add(control);

                    await Task.Delay(200);
                    myScroll.ChangeView(0, myScroll.ExtentHeight, 1);
                }

            }
            catch (Exception ex)
            {

            }

            try
            {
                //if any answer do it here
                if (PenQuestions != null && PenQuestions.Count > 0)
                {
                    switch (PenQuestions.FirstOrDefault().Type)
                    {
                        case "person":
                            var count = lastIdentifiedPersonSample.Count();
                            if (count > 0)
                            {
                                foreach (var item in lastIdentifiedPersonSample)
                                {
                                    if (item.Item2 != null)
                                    {
                                        if (item.Item2.Person.Name.ToLower().Contains("rami"))
                                        {
                                            answertext = "Yes,Rami is on site in sector 7";
                                        }
                                    }
                                }
                                if (answertext == "")
                                {
                                    answertext = "No, Rami is not currently on site in Sector 7";
                                }
                            }


                            break;

                        case "object":
                            break;

                        case "hammer":

                            if (Constants.materials.Count > 0)
                            {
                                foreach (var item in Constants.materials)
                                {
                                    if (item.Confidence.ToLower().Contains("hammer"))
                                    {
                                        ishammer = true;

                                    }

                                }
                            }

                            if (ishammer)
                            {
                                answertext = "Yes, the Hammer is on stand in Sector 7";

                            }
                            else
                            {
                                answertext = "No, The Hammer is not detected in Sector 7";
                            }
                            break;

                        default:
                            answertext = "Sorry, I cannot detect right now";
                            PenQuestions.Clear();


                            break;
                    }
                }
                if (answertext != "")
                {
                    ChatUserControl control = new ChatUserControl();
                    control.setControlProperties(280, "USER RESPONSE", answertext, "blue", null, false);
                    ChatStack.Children.Add(control);

                    await Task.Delay(200);
                    myScroll.ChangeView(0, myScroll.ExtentHeight, 1);
                }

            }
            catch (Exception ex)
            {

            }

        }
        public List<QAs> PenQuestions;
        public List<Syncmessages> messages;
        private async void getNewChats()

        {
            try
            {
                var data = await getData("http://gitex2017backend.azurewebsites.net/api/ServiceRequest/GetPendingQuestions");

                var values = JsonConvert.DeserializeObject<PendingChats>(data);
                messages = values.sync;
                PenQuestions = values.qa;

            }
            catch (Exception ex)
            {

            }

        }

        private async void SavePicturesToServer(ImageAnalyzer imageAnalyzer)
        {

            try
            {
                string base64 = Convert.ToBase64String(imageAnalyzer.Data);

                ImageData _imagedata = new ImageData();

                _imagedata.Data = base64;
                _imagedata.Name = DateTime.Now.ToString("hmmssff");


                var datatosend = JsonConvert.SerializeObject(_imagedata);

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://gitex.eastus.cloudapp.azure.com/gitexbackend/api/values/GetFaceRectangle", new StringContent(datatosend, Encoding.UTF8, "application/json"));

                var result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {

            }



        }

        private ChatTemplateModel returnchat(string textname)
        {
            ChatTemplateModel model = new ChatTemplateModel();
            string answertext = "";
            if (lastIdentifiedPersonSample != null)
            {
                switch (textname)
                {
                    case "hatim":

                        foreach (var item in lastIdentifiedPersonSample)
                        {
                            if (item.Item2 != null)
                            {
                                if (item.Item2.Person.Name.Contains("Hatim"))
                                {
                                    answertext = "Yes,Hatim is looking at the camera";
                                }
                            }
                        }
                        if (answertext == "")
                        {
                            answertext = "No, Hatim is not currently in the picture";
                        }
                        break;
                    case "rami":

                        foreach (var item in lastIdentifiedPersonSample)
                        {
                            if (item.Item2 != null)
                            {
                                if (item.Item2.Person.Name.Contains("Rami"))
                                {
                                    answertext = "yes, Rami is smiling while looking at this message";
                                }
                            }
                        }
                        if (answertext == "")
                        {
                            answertext = "No, Rami is not currently in the picture";
                        }
                        break;

                    default:
                        answertext = "no one is in the picture";
                        break;
                }
            }
            else
            {
                answertext = "no one is in the picture";
            }

            model.Comment = answertext;

            model.isAdmin = Visibility.Visible;
            model.isUser = Visibility.Collapsed;
            model.TextColumn = 1;
            model.Alignment = TextAlignment.Left;
            model.Backcolor = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Gray);

            return model;
        }

        private void CameraControl_CameraAspectRatioChanged(object sender, EventArgs e)
        {
            this.UpdateCameraHostSize();
        }

        private void StartProcessingLoop()
        {
            this.isProcessingLoopInProgress = true;

            if (this.processingLoopTask == null || this.processingLoopTask.Status != TaskStatus.Running)
            {
                this.processingLoopTask = Task.Run(() => this.ProcessingLoop());
            }
        }


        private async void ProcessingLoop()
        {
            while (this.isProcessingLoopInProgress)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (!this.isProcessingPhoto)
                    {
                        //if (DateTime.Now.Hour != this.demographics.StartTime.Hour)
                        //{
                        //    // We have been running through the hour. Reset the data...
                        //    await this.ResetDemographicsData();
                        //    //    this.UpdateDemographicsUI();
                        //}

                        this.isProcessingPhoto = true;
                        if (this.cameraControl.NumFacesOnLastFrame == 0)
                        {
                            await this.ProcessCameraCapture(await this.cameraControl.CaptureFrameAsync());
                        }
                        else
                        {
                            try
                            {
                                await this.ProcessCameraCapture(await this.cameraControl.CaptureFrameAsync());

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }
                });

                //await Task.Delay(1000);
                await Task.Delay(800);
            }
        }

        private async void CurrentWindowActivationStateChanged(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if ((e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.CodeActivated ||
                e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.PointerActivated) &&
                this.cameraControl.CameraStreamState == Windows.Media.Devices.CameraStreamState.Shutdown)
            {
                // When our Window loses focus due to user interaction Windows shuts it down, so we 
                // detect here when the window regains focus and trigger a restart of the camera.
                await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
            }
        }

        BitmapImage fireimage = new BitmapImage();
        private async Task<ImageAnalyzer> GetPrimaryFaceFromCameraCaptureAsync(ImageAnalyzer img, FaceRectangle _facerect)
        {
            if (img == null)
            {
                return null;
            }

            await img.DetectFacesAsync();

            if (img.DetectedFaces == null || !img.DetectedFaces.Any())
            {
                return null;
            }

            // Crop the primary face and return it as the result
            FaceRectangle rect = _facerect;
            double heightScaleFactor = 1.8;
            double widthScaleFactor = 1.8;
            Rectangle biggerRectangle = new Rectangle
            {
                Height = Math.Min((int)(rect.Height * heightScaleFactor), img.DecodedImageHeight),
                Width = Math.Min((int)(rect.Width * widthScaleFactor), img.DecodedImageWidth)
            };
            biggerRectangle.Left = Math.Max(0, rect.Left - (int)(rect.Width * ((widthScaleFactor - 1) / 2)));
            biggerRectangle.Top = Math.Max(0, rect.Top - (int)(rect.Height * ((heightScaleFactor - 1) / 1.4)));

            StorageFile tempFile = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(
                                                    "FaceRecoCameraCapture.jpg",
                                                    CreationCollisionOption.GenerateUniqueName);

            await Util.CropBitmapAsync(img.GetImageStreamCallback, biggerRectangle, tempFile);

            return new ImageAnalyzer(tempFile.OpenStreamForReadAsync, tempFile.Path);
        }

        private async Task ProcessCameraCapture(ImageAnalyzer e)
        {
            try
            {
                if (e == null)
                {
                    this.lastDetectedFaceSample = null;
                    this.lastIdentifiedPersonSample = null;
                    this.lastSimilarPersistedFaceSample = null;
                    this.lastEmotionSample = null;
                    this.debugText.Text = "";

                    this.isProcessingPhoto = false;
                    return;
                }
                await Task.WhenAll(e.DetectFacesAsync(detectFaceAttributes: true));
                //await Task.WhenAll(e.DetectFacesAsync(detectFaceAttributes: true), e.AnalyzeImageAsync(detectCelebrities: false));


                DateTime start = DateTime.Now;
                await Task.Run(() => SavePicturesToServer(e));
                // Compute Emotion, Age and Gender
                //await Task.WhenAll(e.DetectEmotionAsync(), e.DetectFacesAsync(detectFaceAttributes: true));



                //check tags



                //foreach (var item in e.AnalysisResult.Description.Tags)
                //{
                //    if (item.Contains("fire"))
                //    {
                //        isfiredetected = true;
                //    }

                //}

                if (Constants.isFireDetected)
                {
                    if(isfiredetected)
                    {
                        var stream = new InMemoryRandomAccessStream();
                        await stream.WriteAsync(e.Data.AsBuffer());
                        stream.Seek(0);
                        await fireimage.SetSourceAsync(stream);
                        await Task.Delay(300);

                        ChatUserControl control = new ChatUserControl();
                        control.setControlProperties(500, "HAZARD", "Fire detected in sector 7", "peach", fireimage, true);
                        ChatStack.Children.Add(control);
                        // createsystemWarning("SYSTEM : ALERT", 400, 400, "Fire Detected in the meeting room", false, false, "sdf", true, "dark");

                        await Task.Delay(200);
                        myScroll.ChangeView(0, myScroll.ExtentHeight, 1);
                        isfiredetected = false;
                    }
                   
                }



                if (e.DetectedFaces == null || !e.DetectedFaces.Any())
                {
                    this.lastDetectedFaceSample = null;
                    LastPersistedCount = 0;

                }
                else
                {
                    this.lastDetectedFaceSample = e.DetectedFaces;
                    //analysis image can be taken here and done
                    if (!ishelmet)
                    {
                        ishelmet = true;

                        ChatUserControl control = new ChatUserControl();
                        control.setControlProperties(280, "SAFETY VIOLATION", "Personnel with no Helmet", "blue", null, false);
                        ChatStack.Children.Add(control);

                        await Task.Delay(200);
                        myScroll.ChangeView(0, myScroll.ExtentHeight, 1);


                        var data = await getData("http://gitex2017backend.azurewebsites.net/api/ServiceRequest/AnswerQuestion?Answer=Personnel with no Helmet&Session=" + SessionID);

                    }



                    if (LastUpdatedDateTime.AddMinutes(4) < DateTime.Now)
                    {
                        ishelmet = true;
                    }
                }

                // Compute Face Identification and Unique Face Ids
                await Task.WhenAll(e.IdentifyFacesAsync(), e.FindSimilarPersistedFacesAsync());

                if (!e.IdentifiedPersons.Any())
                {
                    this.lastIdentifiedPersonSample = null;
                }
                else
                {
                    this.lastIdentifiedPersonSample = e.DetectedFaces.Select(f => new Tuple<Face, IdentifiedPerson>(f, e.IdentifiedPersons.FirstOrDefault(p => p.FaceId == f.FaceId)));
                }

                if (!e.SimilarFaceMatches.Any())
                {
                    this.lastSimilarPersistedFaceSample = null;
                }
                else
                {
                    this.lastSimilarPersistedFaceSample = e.SimilarFaceMatches;
                }

                int count = 0;
                unauth = true;
                if (lastIdentifiedPersonSample == null && e.DetectedFaces.Count() > 0)
                {
                    var croppedimage = await GetPrimaryFaceFromCameraCaptureAsync(e, e.DetectedFaces.FirstOrDefault().FaceRectangle);
                    if (croppedimage != null)
                    {
                        this.OnImageSearchCompleted(croppedimage);
                    }
                }
                else
                {
                    foreach (var item in lastIdentifiedPersonSample)
                    {
                        try
                        {
                            if (item.Item2 == null)
                            {
                                count++;
                                var croppedImage = e;

                                croppedImage = await GetPrimaryFaceFromCameraCaptureAsync(e, item.Item1.FaceRectangle);

                                if (croppedImage != null)
                                {
                                    this.OnImageSearchCompleted(croppedImage);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                }
                if (!unauth)
                {
                    unauth = true;

                    ChatUserControl control = new ChatUserControl();
                    control.setControlProperties(200, "SECURITY VIOLATION", "Unauthorized personnel on Sector 7", "peach", null, false);
                    ChatStack.Children.Add(control);
                    myScroll.ChangeView(0, myScroll.ExtentHeight, 1);
                }

                if (LastPersistedCount < count)
                {
                    // PopToast(string.Format("{0} Unauthorized faces found", count));
                }
                LastPersistedCount = count;
                //this.UpdateDemographics(e);

                this.debugText.Text = string.Format("Latency: {0}ms", (int)(DateTime.Now - start).TotalMilliseconds);
                this.ErrorText.Text = e.ErrorMessage;
                this.isProcessingPhoto = false;
            }
            catch (Exception ex)
            {
                this.isProcessingPhoto = false;
            }

        }
        bool clickingImages = true;
        ObservableCollection<string> imagelist = new ObservableCollection<string>();
        //to be added when i complete the image cropping
        private async void OnImageSearchCompleted(ImageAnalyzer args)
        {
            //this.progressControl.IsActive = true;

            //this.trainingImageCollectorFlyout.Hide();

            try
            {
                if (clickingImages)
                {
                    if (noofImages < 6)
                    {
                        if (args.GetImageStreamCallback != null)
                        {
                            imagelist.Add(args.LocalImagePath);
                            AddPersistedFaceResult addResult;
                            addResult = await FaceServiceHelper.AddPersonFaceAsync(
                                   groupid,
                                    persionID,
                                     imageStreamCallback: args.GetImageStreamCallback,
                                     userData: args.LocalImagePath,
                                     targetFace: null);
                            noofImages++;
                        }

                        if (noofImages > 1)
                        {
                            if (UnauthorizedGrid.Visibility == Visibility.Collapsed)
                                UnauthorizedGrid.Visibility = Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (groupid == null)
                {
                    getPredefinedStuff();
                }

            }
        }




        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            EnterKioskMode();
            SessionID = Guid.NewGuid().ToString();
            Constants.isDashboard = false;
            Constants.isFireDetected = false;
            if (string.IsNullOrEmpty(SettingsHelper.Instance.EmotionApiKey) || string.IsNullOrEmpty(SettingsHelper.Instance.FaceApiKey))
            {
                //await new MessageDialog("Missing Face or Emotion API Key. Please enter a key in the Settings page.", "Missing API Key").ShowAsync();
            }
            else
            {
                FaceListManager.FaceListsUserDataFilter = SettingsHelper.Instance.WorkspaceKey + "_RealTime";
                await FaceListManager.Initialize();

                //  await ResetDemographicsData();
                //  this.UpdateDemographicsUI();

                await this.cameraControl.StartStreamAsync(isForRealTimeProcessing: true);
                this.StartProcessingLoop();
            }

            base.OnNavigatedTo(e);
        }

        private void UpdateDemographics(ImageAnalyzer img)
        {
            if (this.lastSimilarPersistedFaceSample != null)
            {
                bool demographicsChanged = false;
                // Update the Visitor collection (either add new entry or update existing)
                foreach (var item in this.lastSimilarPersistedFaceSample)
                {
                    Visitor visitor;
                    if (this.visitors.TryGetValue(item.SimilarPersistedFace.PersistedFaceId, out visitor))
                    {
                        visitor.Count++;
                    }
                    else
                    {
                        demographicsChanged = true;

                        visitor = new Visitor { UniqueId = item.SimilarPersistedFace.PersistedFaceId, Count = 1 };
                        this.visitors.Add(visitor.UniqueId, visitor);
                        this.demographics.Visitors.Add(visitor);

                        // Update the demographics stats. We only do it for new visitors to avoid double counting. 
                        AgeDistribution genderBasedAgeDistribution = null;
                        if (string.Compare(item.Face.FaceAttributes.Gender, "male", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.demographics.OverallMaleCount++;
                            genderBasedAgeDistribution = this.demographics.AgeGenderDistribution.MaleDistribution;
                        }
                        else
                        {
                            this.demographics.OverallFemaleCount++;
                            genderBasedAgeDistribution = this.demographics.AgeGenderDistribution.FemaleDistribution;
                        }

                        if (item.Face.FaceAttributes.Age < 16)
                        {
                            genderBasedAgeDistribution.Age0To15++;
                        }
                        else if (item.Face.FaceAttributes.Age < 20)
                        {
                            genderBasedAgeDistribution.Age16To19++;
                        }
                        else if (item.Face.FaceAttributes.Age < 30)
                        {
                            genderBasedAgeDistribution.Age20s++;
                        }
                        else if (item.Face.FaceAttributes.Age < 40)
                        {
                            genderBasedAgeDistribution.Age30s++;
                        }
                        else if (item.Face.FaceAttributes.Age < 50)
                        {
                            genderBasedAgeDistribution.Age40s++;
                        }
                        else
                        {
                            genderBasedAgeDistribution.Age50sAndOlder++;
                        }
                    }
                }

                if (demographicsChanged)
                {
                    //this.ageGenderDistributionControl.UpdateData(this.demographics);
                }

                //this.overallStatsControl.UpdateData(this.demographics);
            }
        }

        //private void UpdateDemographicsUI()
        //{
        //    this.ageGenderDistributionControl.UpdateData(this.demographics);
        //    this.overallStatsControl.UpdateData(this.demographics);
        //}

        private async Task ResetDemographicsData()
        {
            this.initializingUI.Visibility = Visibility.Visible;
            this.initializingProgressRing.IsActive = true;

            //this.demographics = new DemographicsData
            //{
            //    StartTime = DateTime.Now,
            //    AgeGenderDistribution = new AgeGenderDistribution { FemaleDistribution = new AgeDistribution(), MaleDistribution = new AgeDistribution() },
            //    Visitors = new List<Visitor>()
            //};

            //this.visitors.Clear();
            //await FaceListManager.ResetFaceLists();

            this.initializingUI.Visibility = Visibility.Collapsed;
            this.initializingProgressRing.IsActive = false;
        }

        public async Task HandleApplicationShutdownAsync()
        {
            await ResetDemographicsData();
        }

        private void EnterKioskMode()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode)
            {
                view.TryEnterFullScreenMode();
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            try
            {
                await FaceServiceHelper.DeletePersonAsync(groupid, persionID);
                await this.ResetDemographicsData();
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Please enter the FACE API Key in settings");
               
            }
            this.isProcessingLoopInProgress = false;
          
            Constants.isHelmet = false;
            Window.Current.Activated -= CurrentWindowActivationStateChanged;
            this.cameraControl.CameraAspectRatioChanged -= CameraControl_CameraAspectRatioChanged;

         

            await this.cameraControl.StopStreamAsync();
            base.OnNavigatingFrom(e);
        }

        private void OnPageSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateCameraHostSize();
        }

        private void UpdateCameraHostSize()
        {
            this.cameraHostGrid.Width = this.cameraHostGrid.ActualHeight * (this.cameraControl.CameraAspectRatio != 0 ? this.cameraControl.CameraAspectRatio : 1.777777777777);
        }

        public EmotionScores GetLastEmotionForFace(BitmapBounds faceBox)
        {
            if (this.lastEmotionSample == null || !this.lastEmotionSample.Any())
            {
                return null;
            }

            return this.lastEmotionSample.OrderBy(f => Math.Abs(faceBox.X - f.FaceRectangle.Left) + Math.Abs(faceBox.Y - f.FaceRectangle.Top)).First().Scores;
        }

        public Face GetLastFaceAttributesForFace(BitmapBounds faceBox)
        {
            if (this.lastDetectedFaceSample == null || !this.lastDetectedFaceSample.Any())
            {
                return null;
            }

            return Util.FindFaceClosestToRegion(this.lastDetectedFaceSample, faceBox);
        }

        public IdentifiedPerson GetLastIdentifiedPersonForFace(BitmapBounds faceBox)
        {
            if (this.lastIdentifiedPersonSample == null || !this.lastIdentifiedPersonSample.Any())
            {
                return null;
            }

            Tuple<Face, IdentifiedPerson> match =
                this.lastIdentifiedPersonSample.Where(f => Util.AreFacesPotentiallyTheSame(faceBox, f.Item1.FaceRectangle))
                                               .OrderBy(f => Math.Abs(faceBox.X - f.Item1.FaceRectangle.Left) + Math.Abs(faceBox.Y - f.Item1.FaceRectangle.Top)).FirstOrDefault();
            if (match != null)
            {
                return match.Item2;
            }

            return null;
        }

        public SimilarPersistedFace GetLastSimilarPersistedFaceForFace(BitmapBounds faceBox)
        {
            if (this.lastSimilarPersistedFaceSample == null || !this.lastSimilarPersistedFaceSample.Any())
            {
                return null;
            }

            SimilarFaceMatch match =
                this.lastSimilarPersistedFaceSample.Where(f => Util.AreFacesPotentiallyTheSame(faceBox, f.Face.FaceRectangle))
                                               .OrderBy(f => Math.Abs(faceBox.X - f.Face.FaceRectangle.Left) + Math.Abs(faceBox.Y - f.Face.FaceRectangle.Top)).FirstOrDefault();

            return match?.SimilarPersistedFace;
        }


        private void PopToast(string text)
        {
            // Generate the toast notification content and pop the toast
            ToastContent content = GenerateToastContent(text);
            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(content.GetXml()));
        }

        public static ToastContent GenerateToastContent(string text)
        {
            return new ToastContent()
            {
                Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Default,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                {
                    new AdaptiveText()
                    {
                        Text = text
                    }
                }
                    }
                },

                Actions = new ToastActionsCustom()
                {


                    Buttons =
            {

                new ToastButtonDismiss()
            }
                }
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //PopToast();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            if (UnauthorizedGrid.Visibility == Visibility.Visible)
            {
                UnauthorizedGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                UnauthorizedGrid.Visibility = Visibility.Visible;

            }
        }

        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }



        private async void AddPersonBtnclick(object sender, RoutedEventArgs e)
        {
            IEnumerable<PersonGroup> personGroups = await FaceServiceHelper.ListPersonGroupsAsync(SettingsHelper.Instance.WorkspaceKey);
            clickingImages = false;
            //Person[] personsInGroup = await FaceServiceHelper.GetPersonsAsync(personGroups.FirstOrDefault().PersonGroupId);

            //CreatePersonResult result = await FaceServiceHelper.CreatePersonAsync(personGroups.FirstOrDefault().PersonGroupId, "Saeed");

            //foreach (var item in imagelist)
            //{
            //    AddPersistedFaceResult addResult;
            //    addResult = await FaceServiceHelper.AddPersonFaceAsync(
            //            personGroups.FirstOrDefault().PersonGroupId,
            //            result.PersonId,
            //             imageUrl: item,
            //             userData: item,
            //             targetFace: null);

            //}




            await FaceServiceHelper.TrainPersonGroupAsync(personGroups.FirstOrDefault().PersonGroupId);

            UnauthorizedGrid.Visibility = Visibility.Collapsed;




            ChatUserControl control = new ChatUserControl();
            control.setControlProperties(250, "AUTHORIZATION : SUCCESS", "A new User has been Authorized", "green", null, false);
            ChatStack.Children.Add(control);


            //createsystemWarning("", 200, 200, "", false, false, "sdf", false, "sdf");
            await Task.Delay(200);
            myScroll.ChangeView(0, myScroll.ExtentHeight, 1);
        }

        private async void ScrollTextBtnClick(object sender, RoutedEventArgs e)
        {

            //IEnumerable<PersonGroup> personGroups = await FaceServiceHelper.ListPersonGroupsAsync(SettingsHelper.Instance.WorkspaceKey);
            //await FaceServiceHelper.TrainPersonGroupAsync(personGroups.FirstOrDefault().PersonGroupId);


            ChatUserControl control = new ChatUserControl();
            control.setControlProperties(250, "SUCCESS", "A new User has been Authorized", "green", null, false);
            ChatStack.Children.Add(control);

            //createsystemWarning("AUTHORIZATION : SUCCESS", 200, 200, "A new User has been Authorized", false, false, "sdf", false, "dark");
            await Task.Delay(200);
            myScroll.ChangeView(0, myScroll.ExtentHeight, 1);
        }

        private async void ReportunAuthorized_Click(object sender, RoutedEventArgs e)
        {

            clickingImages = false;
            ChatUserControl control = new ChatUserControl();
            control.setControlProperties(250, "SECURITY VIOLATION", "Unauthorized user was Reported successfully", "peach", null, false);
            ChatStack.Children.Add(control);

            // createsystemWarning("UNAUTHORIZED USER : REPORTED", 200, 200, "Unauthorized user was Reported successfully", false, false, "sdf", false, "sdf");
            await Task.Delay(200);
            myScroll.ChangeView(0, myScroll.ExtentHeight, 1);

        }

        private void TextBlock_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            UnauthorizedGrid.Visibility = Visibility.Collapsed;
        }

        private async void add_Click(object sender, RoutedEventArgs e)
        {
            Constants.h = width.Text;
            Constants.y = height.Text;

            //AddPersistedFaceResult addResult;

            //addResult = await FaceServiceHelper.AddPersonFaceAsync(
            //              personGroups.FirstOrDefault().PersonGroupId,
            //              result.PersonId,
            //               imageUrl: "https://infive.ae/wp-content/uploads/events/speakers/17/file-12.jpeg",
            //               userData: "https://infive.ae/wp-content/uploads/events/speakers/17/file-12.jpeg",
            //               targetFace: null);

            //AddPersistedFaceResult addResult1;

            //addResult1 = await FaceServiceHelper.AddPersonFaceAsync(
            //              personGroups.FirstOrDefault().PersonGroupId,
            //              result.PersonId,
            //               imageUrl: "https://avatars2.githubusercontent.com/u/508960?v=4&s=460",
            //               userData: "https://avatars2.githubusercontent.com/u/508960?v=4&s=460",
            //               targetFace: null);




        }
    }

    public abstract class TemplateSelector : ContentControl
    {
        public abstract DataTemplate SelectTemplate(object item, DependencyObject container);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

    //public class ContactTemplateSelector : TemplateSelector
    //{
    //    public DataTemplate ImageLeft
    //    {
    //        get;
    //        set;
    //    }

    //    public DataTemplate ImageRight
    //    {
    //        get;
    //        set;
    //    }


    //    //public override DataTemplate SelectTemplate(object item, DependencyObject container)
    //    //{
    //    //    var warningmessage = item as MessageItems;
    //    //    if(warningmessage.isAdmin)
    //    //    {

    //    //    }
    //    //    else
    //    //    {

    //    //    }


    //    //    // Determine which template to return;
    //    //}
    //}

    public class MessageItems
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public DateTime time { get; set; }
        public bool isAdmin { get; set; }
    }
    [XmlType]
    public class Visitor
    {
        [XmlAttribute]
        public Guid UniqueId { get; set; }

        [XmlAttribute]
        public int Count { get; set; }
    }

    [XmlType]
    public class AgeDistribution
    {
        public int Age0To15 { get; set; }
        public int Age16To19 { get; set; }
        public int Age20s { get; set; }
        public int Age30s { get; set; }
        public int Age40s { get; set; }
        public int Age50sAndOlder { get; set; }
    }

    [XmlType]
    public class AgeGenderDistribution
    {
        public AgeDistribution MaleDistribution { get; set; }
        public AgeDistribution FemaleDistribution { get; set; }
    }

    [XmlType]
    [XmlRoot]
    public class DemographicsData
    {
        public DateTime StartTime { get; set; }

        public AgeGenderDistribution AgeGenderDistribution { get; set; }

        public int OverallMaleCount { get; set; }

        public int OverallFemaleCount { get; set; }

        [XmlArrayItem]
        public List<Visitor> Visitors { get; set; }
    }

    public class tblChat
    {
        public long Id { get; set; }
        public string CommentType { get; set; }
        public string Comment { get; set; }
        public bool isActive { get; set; }
        public System.DateTime CreatedDateTime { get; set; }
        public string SessionName { get; set; }
    }

    public class ImageData
    {
        public string Name { get; set; }
        public string Data { get; set; }
    }

}