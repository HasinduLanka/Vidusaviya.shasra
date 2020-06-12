
var SegmentLength = 4000;

var Live;
var LiveStream;
var LiveReady = false;


var midcanvas;

var mediaRecorder;

var IsStreaming = false;
var recordedChunks = [];
var ChunkCount = 0;

var Viewer;
var Playlist = [];
var IsPlaying = false;
var mediaSource;
var msurl;
var sourceBuffer;

window.InitializeViewer = async () => {

    console.log("InitializeViewer");

    Viewer = document.getElementById('Viewer');

    //mediaSource = new MediaSource();
    //msurl = URL.createObjectURL(mediaSource);
    //Viewer.src = msurl;
    //Viewer.crossOrigin = 'anonymous';
    //

    //sourceBuffer = null;

    //mediaSource.addEventListener("sourceopen", function () {
    //    // NOTE: Browsers are VERY picky about the codec being EXACTLY
    //    // right here. Make sure you know which codecs you're using!
    //    sourceBuffer = mediaSource.addSourceBuffer("video/webm; codecs=\"opus,vp8\"");
    //    sourceBuffer.mode = 'sequence';

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

    // Viewer.oncanplay = e => Viewer.play();


    // var ctx = midcanvas.getContext('2d');



    //Viewer.addEventListener('play', function () {
    //    var $this = this; //cache

    //    (function loop() {
    //        if (!$this.paused && !$this.ended) {
    //            midcanvas.height = $this.videoHeight;
    //            midcanvas.width = $this.videoWidth;
    //            ctx.drawImage($this, 0, 0, $this.videoWidth, $this.videoHeight);
    //            setTimeout(loop, 1000 / 16); // drawing at 8 fps
    //        }
    //    })();
    //}, 0);
}


window.InitializeStreamer = async () => {

    console.log("InitializeStreamer");

    LiveReady = false;
    Live = document.getElementById('live');

    midcanvas = document.getElementById('MidCanvas');

}


window.OpenCam = async () => {
    var CamFace = 'F'
    var frontCam = false;
    if (CamFace == 'F') {
        frontCam = true
    }

    LiveReady = false;

    // Get access to the camera!
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({
            video: {
                width: { min: 360, ideal: 640, max: 720 },
                height: { min: 270, ideal: 360, max: 480 },
                facingMode: (frontCam ? "user" : "environment"),
                frameRate: { ideal: 6, max: 8 }
            }, audio: {
                echoCancellation: true
            }
        }).then(
            function (stream) {

                Live.srcObject = stream;
                LiveStream = stream;

                LiveReady = true;
                Live.play();

                var ctx = midcanvas.getContext('2d');

                Live.addEventListener('play', function () {
                    var $this = this; //cache

                    (function loop() {
                        if (!$this.paused && !$this.ended) {
                            midcanvas.height = $this.videoHeight;
                            midcanvas.width = $this.videoWidth;
                            ctx.drawImage($this, 0, 0, $this.videoWidth, $this.videoHeight);
                            setTimeout(loop, 1000 / 16); // drawing at 8 fps
                        }
                    })();
                }, 0);


                console.log("Res " + Live.videoWidth + "x" + Live.videoHeight);

            });
    }
}

window.IsLiveReady = async () => {
    if (LiveReady == true) { return "R" }
    else { return "N" }
}



window.CloseLive = async () => {

    LiveReady = false;
    LiveStream = midcanvas.captureStream(8);


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
    SegmentLength = 4000;

    if (LiveReady == false) {
        LiveStream = midcanvas.captureStream(8);
    }
    mediaRecorder = new MediaRecorder(LiveStream, options);
    mediaRecorder.ondataavailable = handleDataAvailableCam;


    mediaRecorder.start(4000);
    console.log(mediaRecorder.state);
    console.log("recorder started");

}




function handleDataAvailableCam(event) {
    if (event.data.size > 0) {
        recordedChunks.push(event.data);
        ChunkCount += 1;

        console.log("Camcorder " + mediaRecorder.state + "  #" + ChunkCount + " data read " + (event.data.size) + " B");

        if (IsStreaming) {
        }
        
    }
}



window.StopStreaming = async () => {
    IsStreaming = false;
    console.log(mediaRecorder.state);
    console.log("recorder stopping");
    mediaRecorder.stop();
}



window.GetRec = async () => {

    if (ChunkCount == 0) {
        return "0";
    } else if (ChunkCount == 1) {
        return "1";
    } else if (ChunkCount > 1) {
        ChunkCount = 0;

        var superBuffer = new Blob(recordedChunks, { type: "video/webm" });

        var blb = window.URL.createObjectURL(superBuffer);

        recordedChunks = [];
        //window.AddVideo(blb);
        console.log("Returning superBuffer " + superBuffer.size);

        return blb;
    }


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

        fetch(Playlist.shift()).then(response => response.arrayBuffer().then(
            ab => sourceBuffer.appendBuffer(ab)
        ));


        //B64toBlob(Playlist.shift()).then(function (B) {
        //     B.arrayBuffer().then(function (AB) {
        //    sourceBuffer.appendBuffer(window.URL.createObjectURL(B));
        //     });
        //});

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



