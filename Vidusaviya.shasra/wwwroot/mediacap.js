var SegmentLength = 4000;
const mime = 'video/webm; codecs="opus,vp8"';

var Live;
var LiveStream;
var LiveReady = false;

var midcanvas;

var mediaRecorder;

var IsStreaming = false;
var recordedChunks = [];
var ChunkCount = 0;

var Viewer;
var IsPlaying = false;
var mediaSource;
var sourceBuffer;

window.InitializeViewer = async () => {
    console.log("InitializeViewer");

    Viewer = document.getElementById("Viewer");
    Viewer.addEventListener("error", () => {
        IsPlaying = false;
        console.log("Viewer Error");
        ResetViewer();
    });
    Viewer.addEventListener("ended", event => {
        IsPlaying = false;
        console.log("Viewer end");
    });

    midcanvas = document.getElementById("MidCanvas");
    var ctx = midcanvas.getContext("2d");

    Viewer.addEventListener(
        "play",
        function () {
            var $this = this; //cache

            (function loop() {
                if (!$this.paused && !$this.ended) {
                    if ($this.videoWidth !== 0) {
                        midcanvas.width = window.innerWidth;
                        midcanvas.height =
                            (midcanvas.width * $this.videoHeight) / $this.videoWidth;
                        ctx.drawImage($this, 0, 0, midcanvas.width, midcanvas.height);
                    }
                }
                setTimeout(loop, 1000 / 8); // drawing at 8 fps
            })();
        },
        0
    );

    ResetViewer();
    // Viewer.oncanplay = e => Viewer.play();
};

async function ResetViewer() {
    IsPlaying = false;

    var ms = new MediaSource();
    ms.onsourceopen = ev => {
        console.log("sourceopen");

        var sb = ms.addSourceBuffer(mime);

        sb.mode = "sequence";
        // sb.onupdate = ev => console.info("update", ev);
        // sb.onabort = ev => console.warn("ABORT", ev);
        // sb.onerror = ev => console.error("ERROR", ev);

        sb.onupdateend = ev => {
            if (ChangingTrack) {
                ChangingTrack = false;
                ResetViewer();
                console.log("Track Changed");
            }

            const endTime = sb.buffered.end(sb.buffered.length - 1);
            console.info(`updateend, end is now ${endTime}`, ev);
        };

        sourceBuffer = sb;
    };

    console.log("SRC");
    Viewer.src = URL.createObjectURL(ms);
    console.log("SRC2");
    Viewer.play();
    mediaSource = ms;
    IsPlaying = true;
}

window.InitializeStreamer = async () => {
    console.log("InitializeStreamer");

    LiveReady = false;
    Live = document.getElementById("live");
};

window.OpenCam = async () => {
    var CamFace = "F";
    var frontCam = false;
    if (CamFace === "F") {
        frontCam = true;
    }

    LiveReady = false;

    // Get access to the camera!
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices
            .getUserMedia({
                video: {
                    width: { min: 360, ideal: 640, max: 720 },
                    height: { min: 270, ideal: 360, max: 480 },
                    facingMode: frontCam ? "user" : "environment",
                    frameRate: { ideal: 6, max: 8 }
                },
                audio: {
                    echoCancellation: true
                }
            })
            .then(function (stream) {
                Live.srcObject = stream;
                LiveStream = stream;

                LiveReady = true;
                Live.play();

                console.log("Res " + Live.videoWidth + "x" + Live.videoHeight);
            });
    }
};

window.IsLiveReady = async () => {
    if (LiveReady === true) {
        return "R";
    } else {
        return "N";
    }
};

window.CloseLive = async () => {
    LiveReady = false;

    if (mediaSource) {
        mediaSource.endOfStream();
    }

    LiveStream = midcanvas.captureStream(8);

    var StopCount = 0;
    if (Live.srcObject) {
        Live.srcObject.getTracks().forEach(function (track) {
            track.stop();
            StopCount++;
        });
    }
    return StopCount.toString();
};

window.StartStreaming = async (videoBitrate, segmentlength) => {
    recordedChunks = [];
    ChunkCount = 0;

    var options = {
        audioBitsPerSecond: 128000,
        videoBitsPerSecond: 1024000,
        mimeType: mime
    };

    IsStreaming = true;
    SegmentLength = 3000;

    if (LiveReady === false) {
        LiveStream = midcanvas.captureStream(8);
    }
    mediaRecorder = new MediaRecorder(LiveStream, options);
    mediaRecorder.ondataavailable = handleDataAvailable;

    mediaRecorder.start(SegmentLength);
    console.log(mediaRecorder.state);
    console.log("recorder started");
};

function handleDataAvailable(event) {
    // console.log("DA");
    if (event.data.size > 0) {
        recordedChunks.push(event.data);
        ChunkCount += 1;

        // mediaRecorder.stop();
        // mediaRecorder.start(SegmentLength);
        console.log(
            "Camcorder " +
            mediaRecorder.state +
            "  #" +
            ChunkCount +
            " data read " +
            event.data.size +
            " B"
        );

        if (IsStreaming) {
            window.GetRec();
        }
    }
}

window.StopStreaming = async () => {
    IsStreaming = false;
    console.log(mediaRecorder.state);
    console.log("recorder stopping");
    mediaRecorder.stop();
};

window.GetRec = async () => {
    if (ChunkCount > 0) {
        ChunkCount = 0;

        var superBuffer = new Blob(recordedChunks, { type: "video/webm" });

        //var blb = window.URL.createObjectURL(superBuffer);

        recordedChunks = [];
        await window.AddSeg(superBuffer);
        console.log("Returning superBuffer " + superBuffer.size);

        return superBuffer;
    }
    return "";
};

var ChangingTrack = false;

window.InitTrack = async () => {
    console.log("InitTrack");
    mediaRecorder.stop();
    mediaRecorder.start(SegmentLength);
    ChangingTrack = true;

    if (IsPlaying) {
        Viewer.play();
        IsPlaying = true;
    }

    return window.GetRec();
};

window.AddTrack = async blb => {
    ChangingTrack = true;
    window.AddSeg(blb);
};

var segc = 0;
window.AddSeg = async blb => {
    if (IsPlaying) {
        Viewer.play();
        IsPlaying = true;
    }

    // segc++;
    // if (segc === 4) {
    //   segc = 0;
    //   return;
    // }

    await window.RecieveBlob(blb);
    // if (IsPlaying === false) {

    //   Playlist.push(blb);
    //   //appendToSourceBuffer();
    //   Viewer.play();

    //   console.log("Playlist started");
    //   IsPlaying = true;
    // } else {
    //   Playlist.push(blb);
    //   //appendToSourceBuffer();
    //   console.log("Added to playlist");
    // }
};

var cache = []; // minicache
window.RecieveBlob = async blb => {
    let data = await Promise.resolve().then(() => blobToArray(blb));

    // mediaSource aren't ready
    if (!sourceBuffer || sourceBuffer.updating) {
        cache.push(...data);
        console.log("Cached " + data.length);
        return;
    }
    if (cache.length) {
        console.log("Recv " + data.length);
        data = [...cache, ...data];
        cache = [];
    }

    console.log("Buff " + data.length);
    sourceBuffer.appendBuffer(Uint8Array.from(data));
    console.log(`Appended buffer. Updating=${sourceBuffer.updating}`);
};

// some support function
function blobToArray(blob) {
    const reader = new FileReader();
    let callback, fallback;
    let promise = new Promise((c, f) => {
        callback = c;
        fallback = f;
    });

    function onLoadEnd(e) {
        reader.removeEventListener("loadend", onLoadEnd, false);
        if (e.error) fallback(e.error);
        else callback([...new Uint8Array(reader.result)]);
    }

    reader.addEventListener("loadend", onLoadEnd, false);
    reader.readAsArrayBuffer(blob);
    return promise;
}

window.SayHi = async name => {
    return "Hi " + name;
};
