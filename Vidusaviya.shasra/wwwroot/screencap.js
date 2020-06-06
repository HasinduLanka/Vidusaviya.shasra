var videoElem;
var startstpElem;

// Options for getDisplayMedia()
var isRec = false;
var displayMediaOptions = {
    video: {
        cursor: "always"
    },
    audio: false
};
function InitScreenRec(video, startStpButton) {
    videoElem = document.getElementById(video);
    startstpElem = document.getElementById(startStpButton);
    startstpElem.addEventListener("click", function () {
        if (isRec) {
            stopCapture();
            isRec = false;
        } else {
            startCapture();
            isRec = true;
        }

    }, false);
    console.log("Screen Capture Init Ok...");
}
async function startCapture() {
    console.log("Starting Screen Capture...");
    try {
        videoElem.srcObject = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions);
        //dumpOptionsInfo();
    } catch (err) {
        console.log("Error: " + err);
    }
}

function stopCapture() {
    console.log("Stopping Screen Capture...");
    let tracks = videoElem.srcObject.getTracks();

    tracks.forEach(track => track.stop());
    videoElem.srcObject = null;
}

function dumpOptionsInfo() {
    const videoTrack = videoElem.srcObject.getVideoTracks()[0];

    //console.info("Track settings:");
    //console.info(JSON.stringify(videoTrack.getSettings(), null, 2));
    //console.info("Track constraints:");
    //console.info(JSON.stringify(videoTrack.getConstraints(), null, 2));
}

