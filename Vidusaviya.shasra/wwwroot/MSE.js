var recorder, sourceBuffer;
const Viewer = document.getElementById("viewer");
const mime = 'video/webm; codecs="opus,vp8"';
let needOffsetUpdate = false;

window.start = async () => {
    let mediaSource = new MediaSource();
    mediaSource.onsourceopen = ev => {
        console.log("sourceopen", ev);
    };
    mediaSource.onsourceended = ev => {
        console.log("sourceended", ev);
    };
    mediaSource.onsourceclose = ev => {
        console.log("sourceclose", ev);
    };

    Viewer.src = URL.createObjectURL(mediaSource);

    var mediaStream;
    if (!mediaStream) {
        mediaStream = await navigator.mediaDevices.getUserMedia({
            video: {
                width: { min: 360, ideal: 640, max: 720 },
                height: { min: 270, ideal: 360, max: 480 },
                frameRate: { ideal: 6, max: 8 }
            },
            audio: {
                echoCancellation: true
            }
        });
    }

    console.log("REC");
    recorder = new MediaRecorder(mediaStream, { mimeType: mime });
    recorder.start(2000);
    await new Promise(r => (recorder.onstart = r));

    recorder.ondataavailable = async event => {
        console.log(`dataavailable size=${event.data.size}`);

        window.sendToAnotherClient(event.data);
    };

    console.log("sourceBuffer1");
    sourceBuffer = mediaSource.addSourceBuffer(mime);
    console.log("sourceBuffer2");

    sourceBuffer.mode = "sequence";
    sourceBuffer.onupdate = ev => console.info("update", ev);
    sourceBuffer.onabort = ev => console.warn("ABORT", ev);
    sourceBuffer.onerror = ev => console.error("ERROR", ev);

    console.log("onupdateend");
    sourceBuffer.onupdateend = ev => {
        const endTime = sourceBuffer.buffered.end(sourceBuffer.buffered.length - 1);
        console.info(`updateend, end is now ${endTime}`, ev);
        if (needOffsetUpdate) {
            sourceBuffer.timestampOffset = endTime;
            needOffsetUpdate = false;
        }
    };

    console.log("PLAY");
    Viewer.play();
};

window.stop = async () => {
    if (recorder) {
        recorder.stop();
    }
    Viewer.stop();
};

window.stopstart = async () => {
    await window.stop();
    await window.start();
};

//another client
var cache = []; // minicache

window.sendToAnotherClient = async edata => {
    let data = await Promise.resolve().then(() => blobToArray(edata));

    // mediaSource dont ready
    if (!sourceBuffer || sourceBuffer.updating) {
        cache.push(...data);
        return;
    }
    if (cache.length) {
        data = [...cache, ...data];
        cache = [];
    }
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
