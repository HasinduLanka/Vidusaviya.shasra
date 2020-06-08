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



var mediaRecorderCam;
var camstream;
var midcanvasCam;



var videoCamPrev;
var PlaylistCam = [];
var IsPlayingCam = false;


window.StartPreviewCam = async () => {

    console.log("Starting Cam preview");

    videoCamPrev = document.getElementById('camPerview');

    videoCamPrev.addEventListener('ended', (event) => {

        if (PlaylistCam.length == 0) {
            IsPlayingCam = false;
            console.log('Cam Video end');
        }
        else {
            videoCamPrev.src = Playlist.shift();
            videoCamPrev.play();
            console.log('Cam Video Replaying');
        }
    });



    var video = document.getElementById('liveCam');

    // Get access to the camera!
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        // Not adding `{ audio: true }` since we only want video now
        navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(
            function (stream) {
            //video.src = window.URL.createObjectURL(stream);

            video.srcObject = stream;

            midcanvasCam = document.getElementById('CompCavasCam');
            var ctx = midcanvasCam.getContext('2d');

            video.addEventListener('play', function () {
                var $this = this; //cache
                (function loop() {
                    if (!$this.paused && !$this.ended) {
                        ctx.drawImage($this, 0, 0, 480, 360);
                        setTimeout(loop, 1000 / 16); // drawing at 30fps
                    }
                })();
            }, 0);


            video.play();
            //camstream = stream;
            camstream = midcanvasCam.captureStream(8);
            return "Say cheese";
        });
    }



}



var IsRecordingCam = false;
var recordedChunksCam = [];
var ChunkCountCam = 0;


function handleDataAvailableCam(event) {
    if (event.data.size > 0) {
        recordedChunksCam.push(event.data);
        ChunkCountCam += 1;

        if (IsRecordingCam) {
            mediaRecorderCam.stop();
            mediaRecorderCam.start(2000);
        }
        console.log("Cam Recorder " + mediaRecorderCam.state + "  #" + ChunkCountCam + " data read " + (event.data.size / 1024) + " KB");
    }
}



window.StartRecCam = async () => {

    recordedChunksCam = [];
    ChunkCountCam = 0;

    var options = {
        audioBitsPerSecond: 128000,
        videoBitsPerSecond: 1024000,
        mimeType: 'video/webm'
    }

    IsRecording = true;

    mediaRecorderCam = new MediaRecorder(camstream, options);
    mediaRecorderCam.ondataavailable = handleDataAvailableCam;

    mediaRecorderCam.start(2000);
    console.log(mediaRecorderCam.state);
    console.log("recorder started");
}




window.StopRecCam = async () => {
    IsRecordingCam = false;
    console.log(mediaRecorderCam.state);
    console.log("recorder stopping");
    mediaRecorderCam.stop();
}



window.GetWCStreamCam = async () => {

    if (ChunkCountCam > 0) {
        ChunkCountCam = 0;

        var superBuffer = new Blob(recordedChunksCam, { type: "video/webm" });

        var blb = window.URL.createObjectURL(superBuffer);

        recordedChunksCam = [];
        window.AppendVideoCam(blb);

        return blb;
    }
    return "";

}

window.AppendVideoCam = async (blb) => {

    if (IsPlayingCam == false) {

        videoCamPrev.src = blb;
        videoCamPrev.play();

        console.log('Playlist started');
        IsPlayingCam = true;
    }
    else {
        PlaylistCam.push(blb);
        console.log('Added to playlist');

    }

}


window.SayHi = async (name) => {
    return "Hi " + name;
}



