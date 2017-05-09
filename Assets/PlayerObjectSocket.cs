using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class PlayerObjectSocket : MonoBehaviour {
    int speed = 10;
    float movePower = 1f;
    float jumpPower = 1f;
    Rigidbody2D rigid;
    bool isJumping = false;
    static byte[] Buffer { get; set; }
    static Socket sck;
	// Use this for initialization
	void Start () {

        rigid = gameObject.GetComponent<Rigidbody2D>();
        sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.68.129"), 6000);

        try
        {
            sck.Connect(endPoint);
        }
        catch
        {
            
        }
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Jump"))
        {
            isJumping = true;
        }

        /*byte[] data = Encoding.UTF8.GetBytes(xMove.ToString());
        sck.Send(data, 0, data.Length, SocketFlags.None);
        byte[] buffer = new byte[8];
        int rec = sck.Receive(buffer, 0, buffer.Length, SocketFlags.None);
        float xMove_rcv = float.Parse(Encoding.UTF8.GetString(buffer));*/
	}

    void FixedUpdate()
    {
        Move();
        Jump();
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
        transform.position += moveVelocity * speed * Time.deltaTime;
    }

    void Jump()
    {
        if (!isJumping)
        {
            return;
        }

        rigid.velocity = Vector2.zero;

        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rigid.AddForce(jumpVelocity, ForceMode2D.Impulse);

        isJumping = false;
    }

}
