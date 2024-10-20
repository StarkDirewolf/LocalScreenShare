import * as dotNetConsts from '../Consts.js'

const videoElem = document.getElementById("video")

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

var debug = true
var isHosting = false
var pc
var localStream

createPeerConnection()

videoElem.onerror = () => {
    console.error(`Error ${videoElem.error.code} details: ${videoElem.error.message}`)
}

/**
 * Processes incoming WebRTC signals.
 * @param {RTCSessionDescription} signal
 */
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
        //case 'bye':
            //if (pc) {
            //    hangup()
            //}
            //break
        default:
            logDebug('unhandled', e)
            break
    }
}

/**
 * Creates a WebRTC connection and registers event handlers.
 * @async
 * @see {RTCSessionDescription}
 * @listens pc#icecandidiate
 * @listens pc#track
 */
async function createPeerConnection() {
    pc = new RTCPeerConnection()

    pc.onicecandidate = e => {

        console.log(`Ice candidate message received.`)
        logDebug(`Ice candidate message:`, e)
        
        if (e.candidate != null) {
            const message = {
                type: 'candidate',
                candidate: null,
            }

            message.candidate = e.candidate.candidate
            message.sdpMid = e.candidate.sdpMid
            message.sdpMLineIndex = e.candidate.sdpMLineIndex

            logDebug(`Sending ice candidiate:`, message)

            sendCandidate(message)
        }
    }

    pc.ontrack = e => {
        videoElem.srcObject = new MediaStream([e.track])
        videoElem.play();

        console.log(`Video stream received.`)
        logDebug(`Track received:`, e)
    }
}

/**
 * Adds stream to peer connection and creates a WebRTC offer.
 * @async
 * @returns {Promise<RTCSessionDescription>} - the WebRTC offer to be used as a remote description by peers.
 */
async function startStreaming() {
    isHosting = true
    console.log(`Stream started.`)

    const track = localStream.getTracks()[0]
    pc.addTrack(track)
    logDebug(`Track added to web RTC connection:`, track)

    const offer = await pc.createOffer()
    await pc.setLocalDescription(offer)
    logDebug(`Offer created and set as local description:`, offer)

    return offer
}

/**
 * Handles an incoming WebRTC offer and returns an answer through DotNet.
 * @async
 * @param {RTCSessionDescription} signal - the offer to set as the remote description.
 */
async function handleOffer(signal) {
    console.log(`Establishing connection with host.`)

    await pc.setRemoteDescription(signal)
    logDebug(`Remote description set:`, signal)

    const answer = await pc.createAnswer()
    await pc.setLocalDescription(answer)
    logDebug(`Answer created and set as local description:`, answer)

    const answerJson = JSON.stringify(answer)
    DotNet.invokeMethodAsync(dotNetConsts.clientNamespace, dotNetConsts.localSdpAnswer, answerJson)
}

/**
 * Handles the peer connection icecandidate event, passing the candidate to DotNet.
 * @async
 * @param {RTCIceCandidate} message
 */
async function sendCandidate(message) {
    const messageJson = JSON.stringify(message)

    if (isHosting) {
        logDebug("Sending candidate as host.")
        DotNet.invokeMethodAsync(dotNetConsts.clientNamespace, dotNetConsts.sendHostCandidate, messageJson)
    }
    else {
        logDebug("Sending candidate as client.")
        DotNet.invokeMethodAsync(dotNetConsts.clientNamespace, dotNetConsts.sendClientCandidate, messageJson)
    }
}

/**
 * As the host, handles receiving the client's answer to the created offer.
 * @async
 * @param {RTCSessionDescription} answer - the answer from the client.
 */
async function handleAnswer(answer) {
    if (!pc) {
        console.error('no peerconnection')
        return
    }

    console.log(`Establishing link with client.`)

    await pc.setRemoteDescription(answer)
    logDebug(`Answer received and set to remote description:`, answer)
}

/**
 * Handles receiving an ice candidate signal and adds it to the peer connection.
 * @async
 * @param {RTCIceCandidate} candidate - the new ice candidate.
 */
async function handleCandidate(candidate) {
    if (!pc) {
        console.error('no peerconnection')
        return
    }

    console.log(`New ice candidate.`);

    while (pc.remoteDescription == null) {
        console.log(`Waiting for remote description to be set...`);
        await new Promise(r => setTimeout(r, 500));
    }

    await pc.addIceCandidate(candidate)
    await pc.addIceCandidate()

    logDebug(`Ice candidate added:`, candidate)
}

/**
 * Prints to console if in debug mode.
 */
function logDebug() {
    if (debug) {
        const args = Array.prototype.slice.call(arguments, 1);
        console.log(arguments[0], args)
    }
}

/**
 * Receives a WebRTC signal from DotNet to be processed.
 * @param {any} signalJson
 */
export async function receiveSignal(signalJson) {
    const signal = JSON.parse(signalJson)
    logDebug(`Signal received:`, signal)

    processSignal(signal)
}

/**
 * Prompts the user to select what to window to share, and starts streaming it.
 * @returns {RTCSessionDescription} - the peer connection offer as JSON.
 */
export async function captureScreen() {
    localStream = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions)
    videoElem.srcObject = localStream

    console.log(`Media capture started.`)
    logDebug(`Local stream:`, localStream)

    const offer = await startStreaming()
    logDebug(`Returning offer to hub.`)

    return JSON.stringify(offer)
}