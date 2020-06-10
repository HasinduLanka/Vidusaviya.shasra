//DrawImage
window.SnapCam = async (src, dest) => {
    var canvas = document.getElementById(dest);
    var context = canvas.getContext('2d');
    var video = document.getElementById(src);

    context.drawImage(video, 0, 0, 320, 240);
}

// Get image as base64 string
window.GetImageDataCam = async (el, format) => {
    let canvas = document.getElementById(el);
    let dataUrl = canvas.toDataURL(format);
    return dataUrl.split(',')[1];
}



var SegmentLength = 4000;




var Live;
var LiveReady = false;

var midstream;
var midcanvas;

var mediaRecorder;

var IsStreaming = false;
var recordedChunks = [];
var ChunkCount = 0;

var Viewer;
var Playlist = [];
var IsPlaying = false;
//var mediaSource;
//var msurl;
//var sourceBuffer;

window.InitializeViewer = async () => {

    console.log("InitializeViewer");

    Viewer = document.getElementById('Viewer');

    //mediaSource = new MediaSource();
    //msurl = URL.createObjectURL(mediaSource);
    //Viewer.src = msurl;
    //Viewer.crossOrigin = 'anonymous';

    //sourceBuffer = null;

    //mediaSource.addEventListener("sourceopen", function () {
    //    // NOTE: Browsers are VERY picky about the codec being EXACTLY
    //    // right here. Make sure you know which codecs you're using!
    //    sourceBuffer = mediaSource.addSourceBuffer("video/webm; codecs=\"opus,vp8\"");

    //    // If we requested any video data prior to setting up the SourceBuffer,
    //    // we want to make sure we only append one blob at a time
    //    sourceBuffer.addEventListener("updateend", appendToSourceBuffer);
    //});



    Viewer.addEventListener('ended', (event) => {

        if (Playlist.length == 0) {
            IsPlaying = false;
            console.log('Cam Video end');
        }
        else {
            // appendToSourceBuffer();
            Viewer.src = Playlist.shift();
            Viewer.play();
            console.log('Cam Video Replaying');
        }
    });

    Viewer.addEventListener('error', () => {
        IsPlaying = false;
        console.log('Cam Video Error');
    });


}


window.InitializeStreamer = async () => {

    console.log("InitializeStreamer");

    LiveReady = false;
    Live = document.getElementById('live');
    midcanvas = document.getElementById('MidCanvas');
    var ctx = midcanvas.getContext('2d');
    midstream = midcanvas.captureStream(8);

    Live.addEventListener('play', function () {
        var $this = this; //cache
        (function loop() {
            if (!$this.paused && !$this.ended) {
                ctx.drawImage($this, 0, 0, 720, 480);
                setTimeout(loop, 1000 / 8); // drawing at 8 fps
            }
        })();
    }, 0);

}


window.OpenCam = async () => {

    LiveReady = false;

    // Get access to the camera!
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(
            function (stream) {

                Live.srcObject = stream;

                LiveReady = true;
                Live.play();

            });
    }
}

window.IsLiveReady = async () => {
    if (LiveReady == true) { return "R" }
    else { return "N" }
}



window.CloseLive = async () => {

    LiveReady = false;

    var StopCount = 0;
    if (Live.srcObject) {
        Live.srcObject.getTracks().forEach(function (track) {
            track.stop();
            StopCount++;
        });
    }
    return StopCount.toString();
}


window.StartStreaming = async (videoBitrate, segmentlength) => {


    recordedChunks = [];
    ChunkCount = 0;

    var options = {
        audioBitsPerSecond: 128000,
        videoBitsPerSecond: videoBitrate,
        mimeType: 'video/webm'
    }

    IsStreaming = true;
    SegmentLength = segmentlength;

    mediaRecorder = new MediaRecorder(midstream, options);
    mediaRecorder.ondataavailable = handleDataAvailableCam;

    mediaRecorder.start(SegmentLength);
    console.log(mediaRecorder.state);
    console.log("recorder started");

}


function handleDataAvailableCam(event) {
    if (event.data.size > 0) {
        recordedChunks.push(event.data);
        ChunkCount += 1;

        if (IsStreaming) {
            mediaRecorder.stop();
            mediaRecorder.start(SegmentLength);
        }
        console.log("Camcorder " + mediaRecorder.state + "  #" + ChunkCount + " data read " + (event.data.size / 1024) + " KB");
    }
}



window.StopStreaming = async () => {
    IsStreaming = false;
    console.log(mediaRecorder.state);
    console.log("recorder stopping");
    mediaRecorder.stop();
}



window.GetRec = async () => {

    if (ChunkCount > 0) {
        ChunkCount = 0;

        var superBuffer = new Blob(recordedChunks, { type: "video/webm" });

        var blb = window.URL.createObjectURL(superBuffer);

        recordedChunks = [];
        //window.AddVideo(blb);

        return blb;
    }
    return "";

}

window.AddVideo = async (blb) => {

    if (IsPlaying == false) {

        Viewer.type = "video/webm";
        Viewer.src = blb;

        Playlist.push(blb);
        //appendToSourceBuffer();
        Viewer.play();

        console.log('Playlist started');
        IsPlaying = true;
    }
    else {
        Playlist.push(blb);
        //appendToSourceBuffer();
        console.log('Added to playlist');

    }

}

// 5. Use `SourceBuffer.appendBuffer()` to add all of your chunks to the video
function appendToSourceBuffer() {
    if (
        mediaSource.readyState === "open" &&
        sourceBuffer &&
        sourceBuffer.updating === false &&
        Playlist.length != 0
    ) {

        B64toBlob(Playlist.shift()).then(function (B) {
            B.arrayBuffer().then(function (AB) {
                sourceBuffer.appendBuffer(AB);
            });
        });

    }

    // Limit the total buffer size to 20 minutes
    // This way we don't run out of RAM
    if (
        Viewer.buffered.length &&
        Viewer.buffered.end(0) - Viewer.buffered.start(0) > 240
    ) {
        sourceBuffer.remove(0, Viewer.buffered.end(0) - 240)
    }
}

function B64toBlob(b64) {
    return fetch(b64).then(r => r.blob());
}


window.SayHi = async (name) => {
    return "Hi " + name;
}



