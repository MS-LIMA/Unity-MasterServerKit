using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    public Text roomText;
    public Text playerCountText;

    public Room Room { get; private set; }

    public void Init(Room room)
    {
        this.Room = room;

        this.roomText.text = room.RoomName;
        this.playerCountText.text = string.Format("{0} / {1}", room.PlayerCount, room.MaxPlayer);
    }
}
