using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public struct Protocol_stat
{
    public bool st_move;
    public bool st_jump;
    public bool st_live;
    public Protocol_stat(bool p1, bool p2, bool p3)
    {
        st_move = p1;
        st_jump = p2;
        st_live = p3;
    }
}

public class PlayerObjectSocket : MonoBehaviour {
    float movePower = 1f;
    float jumpPower = 6f;
    Rigidbody2D rigid;
    bool isJumping = false;
    static byte[] Buffer { get; set; }
    static Socket sck;
    IPEndPoint serverEP, sender;
    EndPoint remoteEP;
    int myStat; // -1 left move, 1 right move, 2 jump, 3 live, 4 death
    int packet_size = sizeof(int)*3;
    Protocol_stat obj_stat;

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 300;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        myStat = 0;
        obj_stat = new Protocol_stat(false, false, false);

        try
        {
            serverEP = new IPEndPoint(IPAddress.Parse("192.168.68.130"), 6030);
            sender = new IPEndPoint(IPAddress.Any, 0);
            remoteEP = (EndPoint)sender;

            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.Connect(serverEP);
            Debug.Log("Connection Success");
        }
        catch
        {
            Debug.Log("Connection Failed");
            return;
        }
        Debug.Log("Start");
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Jump"))
        {
            isJumping = true;
        }
        Move();
        Jump();
        getMove();
        getJump();
        /*byte[] data = Encoding.UTF8.GetBytes(xMove.ToString());
        sck.Send(data, 0, data.Length, SocketFlags.None);
        byte[] buffer = new byte[8];
        int rec = sck.Receive(buffer, 0, buffer.Length, SocketFlags.None);
        float xMove_rcv = float.Parse(Encoding.UTF8.GetString(buffer));*/
    }

    void FixedUpdate()
    {
        //Move();
        //Jump();
    }

    void Move()
    {
        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetAxisRaw("Horizontal") < 0)
        { 
            moveVelocity = Vector3.left;
        }else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            moveVelocity = Vector3.right;
        }
        //Debug.Log("Moving");
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            myStat = (int)moveVelocity.x;
            Debug.Log("Move func : " + (int)moveVelocity.x);
            byte[] data = GetBytes_Bind(myStat);
            sck.Send(data, 0, data.Length, SocketFlags.None);
            obj_stat.st_move = true;
        }        

    }

    void Jump()
    {
        if (!isJumping)
        {
            return;
        }
       
        rigid.velocity = Vector2.zero;
        myStat = 2;
        byte[] data = GetBytes_Bind(myStat);
        sck.Send(data, 0, data.Length, SocketFlags.None);
        isJumping = false;
        obj_stat.st_jump = true;
    }

    void getMove()
    {
        if (obj_stat.st_move)
        {
            Vector3 moveVelocity = Vector3.zero;
            byte[] buffer = new byte[packet_size + 1];
            int rec = sck.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            int recv_pack = GetBindAck(buffer);
            if (recv_pack == 1)
                moveVelocity.x = 1;
            else if (recv_pack == -1)
                moveVelocity.x = -1;
            /*byte[] data = Encoding.UTF8.GetBytes(myStat.ToString());
            sck.Send(data, 0, data.Length, SocketFlags.None);
            byte[] buffer = new byte[128];
            int rec = sck.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            float xMove_rcv = float.Parse(Encoding.UTF8.GetString(buffer));
            moveVelocity.x = xMove_rcv;
            Debug.Log(xMove_rcv);*/

            if (recv_pack == 1 || recv_pack == -1)
            {
                //Vector3 targetPosition = new Vector3(transform.position.x + recv_pack.xMove, 0, 0);
                transform.position += moveVelocity * movePower * Time.deltaTime;
                //transform.position = Vector3.Lerp(transform.position, targetPosition, 2*Time.deltaTime);
            }
            obj_stat.st_move = false;
        }
    }

    void getJump()
    {
        if (obj_stat.st_jump)
        {
            Vector2 jumpVelocity = new Vector2(0, 0);
            byte[] buffer = new byte[packet_size + 1];
            int rec = sck.Receive(buffer, 0, buffer.Length, SocketFlags.None);
            int recv_pack = GetBindAck(buffer);
            if (recv_pack == 2)
                jumpVelocity.y = 5;
            Debug.Log(recv_pack);
            rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);
            obj_stat.st_jump = false;
        }


    }

    byte[] GetBytes_Bind(int packet)
    {
        byte[] btBuffer = new byte[packet_size+1];

        MemoryStream ms = new MemoryStream(btBuffer, true);
        BinaryWriter bw = new BinaryWriter(ms);

        bw.Write(IPAddress.HostToNetworkOrder(packet));
        Debug.Log("1 "+ IPAddress.HostToNetworkOrder(packet));

        bw.Close();
        ms.Close();

        return btBuffer;
    }

    int GetBindAck(byte[] btbuffer)
    {
        int packet;

        MemoryStream ms = new MemoryStream(btbuffer, false);
        BinaryReader br = new BinaryReader(ms);
        
        packet = IPAddress.NetworkToHostOrder(br.ReadInt32());
        Debug.Log("test x " + packet);

        br.Close();
        ms.Close();
        return packet;
    }

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Obstacle_acid")
        {
            Destroy(gameObject);
            Debug.Log("Destroy");
        }
    }
}
