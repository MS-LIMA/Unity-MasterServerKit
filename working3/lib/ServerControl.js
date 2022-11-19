var lobbies = {};


////////////////////////////////////////////////
const onSocketMessage = (data) => {
    let json = JSON.parse(message);

}

const onSocketClose = (data) => {

}

////////////////////////////////////////////////

module.exports = {
    onSocketMessage,
    onSocketClose
}