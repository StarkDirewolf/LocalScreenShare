const videoElem = document.getElementById("video");

let pc;
let localStream;
let receiveStream;

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
//signaling.onmessage = e => {
//    switch (e.data.type) {
//        case 'offer':
//            handleOffer(e.data);
//            break;
//        case 'answer':
//            handleAnswer(e.data);
//            break;
//        case 'candidate':
//            handleCandidate(e.data);
//            break;
//        case 'ready':
//            // A second tab joined. This tab will initiate a call unless in a call already.
//            if (pc) {
//                console.log('already in call, ignoring');
//                return;
//            }
//            makeCall();
//            break;
//        case 'bye':
//            if (pc) {
//                hangup();
//            }
//            break;
//        default:
//            console.log('unhandled', e);
//            break;
//    }
//};

videoElem.onerror = () => {
    console.log(`Error ${videoElem.error.code}; details: ${videoElem.error.message}`)
}

function processSignal(signal) {
    switch (signal.type) {
        case 'offer':
            handleOffer(signal);
            break;
        case 'answer':
            handleAnswer(signal);
            break;
        case 'candidate':
            handleCandidate(signal);
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
}

async function createPeerConnection() {
    pc = new RTCPeerConnection();

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
        processSignal(message);
    };
    //pc.ontrack = e => videoElem.srcObject = e.streams[0];
}

async function makeCall() {
    await createPeerConnection();

    const offer = await pc.createOffer();
    processSignal(offer);
    await pc.setLocalDescription(offer);
    DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdp', offer);
}

async function handleOffer(signal) {
    if (pc) {
        console.error('existing peerconnection');
        return;
    }
    await createPeerConnection();
    await pc.setRemoteDescription(signal);

    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);

    pc.ontrack = e => videoElem.srcObject = e.streams[0];

    const answerJson = JSON.stringify(answer);
    DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdpAnswerAsync', answerJson);
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

export async function receiveSignal(signalJson) {
    var signal = JSON.parse(signalJson);
    processSignal(signal);
}

export async function captureScreen() {
    localStream = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions);
    videoElem.srcObject = localStream;
    await createPeerConnection();

    localStream.onaddtrack = e => {
        console.log("track");
        pc.addTrack(e.track);
    }

    console.log(localStream);

    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);
    /*DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdp', offer);*/
    return JSON.stringify(offer);
}