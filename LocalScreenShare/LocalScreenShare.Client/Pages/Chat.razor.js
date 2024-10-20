const videoElem = document.getElementById("video");

let pc;
let localStream;

const displayMediaOptions = {
    video: {
        displaySurface: "window",
        frameRate: 10,
        width: { ideal: 720 },
        height: { ideal: 1024 }
    },
    audio: {
        suppressLocalAudioPlayback: false,
    },
    preferCurrentTab: false,
    selfBrowserSurface: "exclude",
    systemAudio: "include",
    surfaceSwitching: "include",
    monitorTypeSurfaces: "include",
};

const signaling = new BroadcastChannel('webrtc');
signaling.onmessage = e => {
    switch (e.data.type) {
        case 'offer':
            handleOffer(e.data);
            break;
        case 'answer':
            handleAnswer(e.data);
            break;
        case 'candidate':
            handleCandidate(e.data);
            break;
        case 'ready':
            // A second tab joined. This tab will initiate a call unless in a call already.
            if (pc) {
                console.log('already in call, ignoring');
                return;
            }
            makeCall();
            break;
        case 'bye':
            if (pc) {
                hangup();
            }
            break;
        default:
            console.log('unhandled', e);
            break;
    }
};

videoElem.onerror = () => {
    console.log(`Error ${videoElem.error.code}; details: ${videoElem.error.message}`)
}

async function createPeerConnection() {
    pc = new RTCPeerConnection();

    if (localStream != null) {
        pc.addStream(localStream);
    }

    pc.onicecandidate = e => {
        const message = {
            type: 'candidate',
            candidate: null,
        };
        if (e.candidate) {
            message.candidate = e.candidate.candidate;
            message.sdpMid = e.candidate.sdpMid;
            message.sdpMLineIndex = e.candidate.sdpMLineIndex;
        }
        signaling.postMessage(message);
    };
    //pc.ontrack = e => videoElem.srcObject = e.streams[0];
}

async function makeCall() {
    await createPeerConnection();

    const offer = await pc.createOffer();
    signaling.postMessage(offer);
    await pc.setLocalDescription(offer);
    DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdp', offer);
}

async function handleOffer(offer) {
    if (pc) {
        console.error('existing peerconnection');
        return;
    }
    await createPeerConnection();
    await pc.setRemoteDescription(offer);

    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);
    DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdpAnswer', answer);
}

async function handleAnswer(answer) {
    if (!pc) {
        console.error('no peerconnection');
        return;
    }
    await pc.setRemoteDescription(answer);
}

async function handleCandidate(candidate) {
    if (!pc) {
        console.error('no peerconnection');
        return;
    }
    if (!candidate.candidate) {
        await pc.addIceCandidate(null);
    } else {
        await pc.addIceCandidate(candidate);
    }
}

export async function receiveAnswer(sdpAnswerJson) {
    var sdp = JSON.parse(sdpAnswerJson);
    signaling.postMessage(sdp);
}

export async function connectRemoteSdp(sdpJson) {
    var sdp = JSON.parse(sdpJson);
    signaling.postMessage(sdp)
}

export async function captureScreen() {
    localStream = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions);

    await createPeerConnection();

    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);
    /*DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdp', offer);*/
    return JSON.stringify(offer);
}