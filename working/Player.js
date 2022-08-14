class Player {
    constructor(id, version, nickname, customProperties, socket){
        this.id = (id) ? id : crypto.randomUUID();
        this.version = version;
        this.nickname = nickname;
        this.roomId = null;
        this.customProperties = customProperties;

        this.socket = socket;
    }

    AddCallbacks(){

    }

    IsInRoom(){
        return (this.roomId) ? true : false;
    }
}

module.exports = Player;