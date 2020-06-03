//DrawImage
window.Snap = async (src, dest) => {
    var canvas = document.getElementById(dest);
    var context = canvas.getContext('2d');
    var video = document.getElementById(src);

    context.drawImage(video, 0, 0, 320, 240);
}

// Get image as base64 string
window.GetImageData = async (el, format) => {
    let canvas = document.getElementById(el);
    let dataUrl = canvas.toDataURL(format);
    return dataUrl.split(',')[1];
}



var mediaRecorder;
var camstream;
var midcanvas;



var video2;
var Playlist = [];
var IsPlaying = false;


window.StartPreview = async () => {

    console.log("Starting preview");

    video2 = document.getElementById('video2');

    video2.addEventListener('ended', (event) => {

        if (Playlist.length == 0) {
            IsPlaying = false;
            console.log('Video end');
        }
        else {
            video2.src = Playlist.shift();
            video2.play();
            console.log('Video Replaying');
        }
    });



    var video = document.getElementById('video');

    // Get access to the camera!
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        // Not adding `{ audio: true }` since we only want video now
        navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then(function (stream) {
            //video.src = window.URL.createObjectURL(stream);

            video.srcObject = stream;

            midcanvas = document.getElementById('midcanvas');
            var ctx = midcanvas.getContext('2d');

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
            camstream = midcanvas.captureStream(8);
            return "Say cheese";
        });
    }



}



var IsRecording = false;
var recordedChunks = [];
var ChunkCount = 0;


function handleDataAvailable(event) {
    if (event.data.size > 0) {
        recordedChunks.push(event.data);
        ChunkCount += 1;

        if (IsRecording) {
            mediaRecorder.stop();
            mediaRecorder.start(4000);
        }
        console.log("Recorder " + mediaRecorder.state + "  #" + ChunkCount + " data read " + (event.data.size / 1024) + " KB");
    }
}



window.StartRec = async () => {

    recordedChunks = [];
    ChunkCount = 0;

    var options = {
        audioBitsPerSecond: 128000,
        videoBitsPerSecond: 1024000,
        mimeType: 'video/webm'
    }

    IsRecording = true;

    mediaRecorder = new MediaRecorder(camstream, options);
    mediaRecorder.ondataavailable = handleDataAvailable;

    mediaRecorder.start(4000);
    console.log(mediaRecorder.state);
    console.log("recorder started");
}




window.StopRec = async () => {
    IsRecording = false;
    console.log(mediaRecorder.state);
    console.log("recorder stopping");
    mediaRecorder.stop();
}



window.GetWCStream = async () => {

    if (ChunkCount > 0) {
        ChunkCount = 0;

        var superBuffer = new Blob(recordedChunks, { type: "video/webm" });

        var blb = window.URL.createObjectURL(superBuffer);


        if (IsPlaying == false) {

            video2.src = blb;
            video2.play();

            console.log('Playlist started');
            IsPlaying = true;
        }
        else {
            Playlist.push(blb);
            console.log('Added to playlist');
        }


        recordedChunks = [];


        return blb;
    }
    return "";



}


window.SayHi = async (name) => {
    return "Hi " + name;
}



