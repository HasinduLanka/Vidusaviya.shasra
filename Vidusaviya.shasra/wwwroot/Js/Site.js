function cancelFullScreen(el) {
    var requestMethod = el.cancelFullScreen || el.webkitCancelFullScreen || el.mozCancelFullScreen || el.exitFullscreen;
    if (requestMethod) { // cancel full screen.
        requestMethod.call(el);
    } else if (typeof window.ActiveXObject !== "undefined") { // Older IE.
        var wscript = new ActiveXObject("WScript.Shell");
        if (wscript !== null) {
            wscript.SendKeys("{F11}");
        }
    }
}

function requestFullScreen(el) {
    // Supports most browsers and their versions.
    var requestMethod = el.requestFullScreen || el.webkitRequestFullScreen || el.mozRequestFullScreen || el.msRequestFullscreen;

    if (requestMethod) { // Native full screen.
        requestMethod.call(el);
    } else if (typeof window.ActiveXObject !== "undefined") { // Older IE.
        var wscript = new ActiveXObject("WScript.Shell");
        if (wscript !== null) {
            wscript.SendKeys("{F11}");
        }
    }
    return false
}

function toggleFull() {
    var elem = document.body; // Make the body go full screen.
    var isInFullScreen = (document.fullScreenElement && document.fullScreenElement !== null) || (document.mozFullScreen || document.webkitIsFullScreen);

    if (isInFullScreen) {
        cancelFullScreen(document);
    } else {
        requestFullScreen(elem);
    }
    return false;
}

function openChat() {
    document.getElementById("chatBox").style.display = "block";
}

function closeChat() {
    document.getElementById("chatBox").style.display = "none";
}

function togleChat() {
    var isopen = document.getElementById("chatBox").style.display == "block";
    if (isopen) {
        closeChat();
    }
    else {
        openChat();
        initChatBox();
    }
}

var disqus_config = function () {
    this.page.url = "https://localhost:5001/meet";  // Replace PAGE_URL with your page's canonical URL variable
    this.page.identifier = 98134989189842949194199489;
};
function initChatBox() {
    var d = document.getElementById("chatBox"), s = document.createElement('script');
    s.src = 'https://vidusaviya-shasthra.disqus.com/embed.js';
    s.setAttribute('data-timestamp', +new Date());
    d.appendChild(s);

}

window.showModal = async (id) => {
    $(id).modal('show');
    console.log("Show Modal");
}