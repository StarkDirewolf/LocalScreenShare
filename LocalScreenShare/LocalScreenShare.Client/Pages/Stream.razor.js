const videoElem = document.getElementById("video")

var host = false
var pc
var localStream

createPeerConnection()

const displayMediaOptions = {
    video: {
        displaySurface: "window",
        frameRate: 30,
        width: { ideal: 2048 },
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
}

videoElem.onerror = () => {
    console.log(`Error ${videoElem.error.code} details: ${videoElem.error.message}`)
}

function processSignal(signal) {
    switch (signal.type) {
        case 'offer':
            handleOffer(signal)
            break
        case 'answer':
            handleAnswer(signal)
            break
        case 'candidate':
            handleCandidate(signal)
            break
        //case 'ready':
        //    // A second tab joined. This tab will initiate a call unless in a call already.
        //    if (pc) {
        //        console.log('already in call, ignoring')
        //        return
        //    }
        //    makeCall()
        //    break
        case 'bye':
            if (pc) {
                hangup()
            }
            break
        default:
            console.log('unhandled', e)
            break
    }
}

async function createPeerConnection() {
    pc = new RTCPeerConnection()

    pc.onicecandidate = e => {

        console.log(`Ice candidate message received:`, e)
        
        if (e.candidate != null) {
            const message = {
                type: 'candidate',
                candidate: null,
            }

            message.candidate = e.candidate.candidate
            message.sdpMid = e.candidate.sdpMid
            message.sdpMLineIndex = e.candidate.sdpMLineIndex

            console.log(`Sending ice candidiate:`, message)

            sendCandidate(message)
        }
    }

    pc.ontrack = e => {
        videoElem.srcObject = new MediaStream([e.track])
        videoElem.play();
        console.log(`Track received:`, e)
    }
    //pc.ontrack = e => videoElem.srcObject = e.streams[0]
}

async function makeCall() {
    let track = localStream.getTracks()[0]
    const sender = pc.addTrack(track)
    console.log(`Track added to web RTC connection:`, track)

    const parameters = sender.getParameters()
    parameters.encodings[0].maxBitrate = 1000
    //parameters.codecs = [{ mimeType: 'video/H264' }]
    await sender.setParameters(parameters);

    host = true
    const offer = await pc.createOffer()
    await pc.setLocalDescription(offer)
    console.log(`Offer created and set as local description:`, offer)

    return offer
}

async function handleOffer(signal) {

    await pc.setRemoteDescription(signal)
    console.log(`Remote description set:`, signal)

    const answer = await pc.createAnswer()
    await pc.setLocalDescription(answer)
    console.log(`Answer created and set as local description:`, answer)

    const answerJson = JSON.stringify(answer)
    DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveLocalSdpAnswerAsync', answerJson)
}

async function sendCandidate(message) {
    let messageJson = JSON.stringify(message)

    if (host) {
        console.log("Sending candidate as host.")
        DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveHostCandidateAsync', messageJson)
    }
    else {
        console.log("Sending candidate as client.")
        DotNet.invokeMethodAsync('LocalScreenShare.Client', 'ReceiveClientCandidateAsync', messageJson)
    }
}

async function handleAnswer(answer) {
    if (!pc) {
        console.error('no peerconnection')
        return
    }

    await pc.setRemoteDescription(answer)
    console.log(`Answer received and set to remote description:`, answer)
}

async function handleCandidate(candidate) {
    if (!pc) {
        console.error('no peerconnection')
        return
    }

    while (pc.remoteDescription == null) {
        console.log(`Waiting for remote description to be set...`);
        await new Promise(r => setTimeout(r, 500));
    }
    //if (!candidate.candidate) {
    //    await pc.addIceCandidate()
    //} else {
    await pc.addIceCandidate(candidate)
    await pc.addIceCandidate()
    // -------------------------------- Is media track being handled too early?
    console.log(`Ice candidate added:`, candidate)
    //}
}

export async function receiveSignal(signalJson) {
    var signal = JSON.parse(signalJson)
    console.log(`Signal received:`, signal)

    processSignal(signal)
}

export async function captureScreen() {
    localStream = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions)
    videoElem.srcObject = localStream
    console.log(`Media capture started:`, localStream)

    const offer = await makeCall()
    console.log(`Returning offer to hub.`)

    return JSON.stringify(offer)
}