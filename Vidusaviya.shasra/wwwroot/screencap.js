////var videoElem;
////var startstpElem;

////// Options for getDisplayMedia()
////var isRec = false;
////var displayMediaOptions = {
////    video: {
////        cursor: "always"
////    },
////    audio: false
////};
////function InitScreenRec(video, startStpButton) {
////    videoElem = document.getElementById(video);
////    startstpElem = document.getElementById(startStpButton);
////    startstpElem.addEventListener("click", function () {
////        if (isRec) {
////            stopCapture();
////            isRec = false;
////        } else {
////            startCapture();
////            isRec = true;
////        }

////    }, false);
////    console.log("Screen Capture Init Ok...");
////}
////async function startCapture() {
////    console.log("Starting Screen Capture...");
////    try {
////        videoElem.srcObject = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions);
////        //dumpOptionsInfo();
////    } catch (err) {
////        console.log("Error: " + err);
////    }
////}

////function stopCapture() {
////    console.log("Stopping Screen Capture...");
////    let tracks = videoElem.srcObject.getTracks();

////    tracks.forEach(track => track.stop());
////    videoElem.srcObject = null;
////}

//var mediaRecorderScreen;
//var screenstream;
//var midcanvasScreen;

//var videoScreenPrev;
//var PlaylistScreen = [];
//var IsPlayingScreen = false;

////DrawImage
//window.SnapScreen = async (src, dest) => {
//    var canvas = document.getElementById(dest);
//    var context = canvas.getContext('2d');
//    var video = document.getElementById(src);

//    context.drawImage(video, 0, 0, 320, 240);
//}

//// Get image as base64 string
//window.GetImageDataScreen = async (el, format) => {
//    let canvas = document.getElementById(el);
//    let dataUrl = canvas.toDataURL(format);
//    return dataUrl.split(',')[1];
//}

//window.StartPreviewScreen = async () => {

//    console.log("Starting Screen Capture preview");

//    videoScreenPrev = document.getElementById('screenPreview');

//    videoScreenPrev.addEventListener('ended', (event) => {

//        if (PlaylistScreen.length == 0) {
//            IsPlayingScreen = false;
//            console.log('Screen Video end');
//        }
//        else {
//            videoScreenPrev.src = PlaylistScreen.shift();
//            videoScreenPrev.play();
//            console.log('Screen Video Replaying');
//        }
//    });

//    var video = document.getElementById('liveScreen');
//    try {
//        var displayMediaOptions = {
//            video: {
//                cursor: "always"
//            },
//            audio: false
//        };
//        navigator.mediaDevices.getDisplayMedia(displayMediaOptions).then(
//            function (stream) {
//                //video.src = window.URL.createObjectURL(stream);

//                video.srcObject = stream;

//                midcanvasScreen = document.getElementById('CompCavasScreen');
//                var ctx = midcanvasScreen.getContext('2d');

//                video.addEventListener('play', function () {
//                    var $this = this; //cache
//                    (function loop() {
//                        if (!$this.paused && !$this.ended) {
//                            ctx.drawImage($this, 0, 0, 480, 360);
//                            setTimeout(loop, 1000 / 16); // drawing at 30fps
//                        }
//                    })();
//                }, 0);

//                video.play();
//                screenstream = midcanvasScreen.captureStream(8);
//                return "Say cheese";
//            });;
//    } catch (err) {
//        console.log("Error: " + err);
//    }
//}

//var IsRecordingScreen = false;
//var recordedChunksScreen = [];
//var ChunkCountScreen = 0;


//function handleDataAvailableScreen(event) {
//    if (event.data.size > 0) {
//        recordedChunksScreen.push(event.data);
//        ChunkCountScreen += 1;

//        if (IsRecordingScreen) {
//            mediaRecorderScreen.stop();
//            mediaRecorderScreen.start(2000);
//        }
//        console.log("Screen Recorder " + mediaRecorderScreen.state + "  #" + ChunkCountScreen + " data read " + (event.data.size / 1024) + " KB");
//    }
//}



//window.StartRecScreen = async () => {

//    recordedChunksScreen = [];
//    ChunkCountScreen = 0;

//    var options = {
//        audioBitsPerSecond: 128000,
//        videoBitsPerSecond: 1024000,
//        mimeType: 'video/webm'
//    }

//    IsRecordingScreen = true;

//    mediaRecorderScreen = new MediaRecorder(screenstream, options);
//    mediaRecorderScreen.ondataavailable = handleDataAvailableScreen;

//    mediaRecorderScreen.start(2000);
//    console.log(mediaRecorderScreen.state);
//    console.log("Screen recorder started");
//}




//window.StopRecScreen = async () => {
//    IsRecordingScreen = false;
//    console.log(mediaRecorderScreen.state);
//    console.log("Screen recorder stopping");
//    mediaRecorderScreen.stop();
//}



//window.GetWCStreamScreen = async () => {

//    if (ChunkCountScreen > 0) {
//        ChunkCountScreen = 0;

//        var superBuffer = new Blob(recordedChunksScreen, { type: "video/webm" });

//        var blb = window.URL.createObjectURL(superBuffer);

//        recordedChunksScreen = [];
//        window.AppendVideoScreen(blb);

//        return blb;
//    }
//    return "";

//}

//window.AppendVideoScreen = async (blb) => {

//    if (IsPlayingScreen == false) {

//        videoScreenPrev.src = blb;
//        videoScreenPrev.play();

//        console.log('Playlist started');
//        IsPlayingScreen = true;
//    }
//    else {
//        PlaylistScreen.push(blb);
//        console.log('Added to playlist');

//    }

//}
















