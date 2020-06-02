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

window.StartPreview = async () => {
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
                        setTimeout(loop, 1000 / 10); // drawing at 30fps
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
var recordedChunks = [];

function handleDataAvailable(event) {
    if (event.data.size > 0) {
        recordedChunks.push(event.data);

        console.log("Recorder " + mediaRecorder.state + " data available " + (event.data.size / 8 / 1024) + " KB/s");
    }
}



window.StartRec = async () => {

    recordedChunks = [];
    var options = {
        audioBitsPerSecond: 128000,
        videoBitsPerSecond: 512000,
        mimeType: 'video/webm'
    }
    mediaRecorder = new MediaRecorder(camstream, options);
    mediaRecorder.ondataavailable = handleDataAvailable;

    mediaRecorder.start(4000);
    console.log(mediaRecorder.state);
    console.log("recorder started");
}

window.StopRec = async () => {
    console.log(mediaRecorder.state);
    console.log("recorder stopping");
    mediaRecorder.stop();
}


window.GetWCStream = async () => {

    var video2 = document.getElementById('video2');
    var superBuffer = new Blob(recordedChunks, { type: "video/webm" });
    recordedChunks = [];

    var blb = window.URL.createObjectURL(superBuffer);
    video2.src = blb;
    return blb;

    // Set the source of one <video> element to be a stream from another.
    //var stream2 = video.captureStream(20);
    //video2.srcObject = stream2;
    //return stream2;

}


window.SayHi = async (name) => {
    return "Hi " + name;
}



