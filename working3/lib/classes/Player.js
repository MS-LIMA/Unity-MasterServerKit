const crypto = require('crypto');

export class Player {
    constructor(id, nickname, socket){
        this.id = id || crypto.randomUUID();
        this.nickname = nickname || this.id;

        this.socket = socket;

        this.roomId = '';
    }
}