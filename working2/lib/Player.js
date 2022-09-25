class Player {
    constructor(version, id, nickname, customProperties, socket){
        this.version = version;
        this.roomId = "";
        this.id = id;
        this.nickname = nickname;
        this.customProperties = customProperties;
        this.socket = socket;
    }

    IsInRoom(){
        return (this.roomId) ? true : false;
    }
}

module.exports = Player;