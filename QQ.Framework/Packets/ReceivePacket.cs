﻿using QQ.Framework.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQ.Framework.Packets
{
    public class ReceivePacket : Packet
    {
        public ReceivePacket() : base()
        {
        }

        public BinaryReader reader;

        public void Decrypt(byte[] key)
        {
            bodyDecrypted = QQTea.Decrypt(buffer, (int) reader.BaseStream.Position,
                (int) (buffer.Length - reader.BaseStream.Position - 1), key);
            if (bodyDecrypted == null)
            {
                throw new Exception($"包内容解析出错，抛弃该包: {ToString()}");
            }

            reader = new BinaryReader(new MemoryStream(bodyDecrypted));
        }

        /// <summary>
        /// 包体长度
        /// </summary>
        /// <returns></returns>
        public int GetPacketLength()
        {
            return bodyEcrypted.Length;
        }

        /// <summary>
        /// 构造一个指定参数的包
        /// </summary>
        /// <param name="byteBuffer"></param>
        /// <param name="User"></param>
        /// <param name="Key">解密Key</param>
        public ReceivePacket(byte[] byteBuffer, QQUser User, byte[] Key)
            : base(byteBuffer, User)
        {
            reader = new BinaryReader(new MemoryStream(buffer));
            bodyEcrypted = byteBuffer;
            //指定随包解密Key
            _secretKey = Key;
            //提取包头部分
            ParseHeader();

            try
            {
                //解析包
                ParseBody();
            }
            catch (Exception e)
            {
                user.MessageLog($"包内容解析出错,错误{e.Message}，包名: {ToString()}");
            }

            //提取包尾部分
            ParseTail();
        }

        /// <summary>
        /// 从buf的当前位置解析包尾
        /// </summary>
        /// <param name="buf">The buf.</param>
        protected void ParseTail()
        {
            try
            {
                reader.ReadByte();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 解析包体，从buf的开头位置解析起
        /// </summary>
        /// <param name="reader">The buf.</param>
        protected virtual void ParseBody()
        {
        }

        /// <summary>
        /// 从buf的当前位置解析包头
        /// </summary>
        /// <param name="reader">The buf.</param>
        protected virtual void ParseHeader()
        {
            Header = reader.ReadByte();
            Version = reader.BEReadChar();
            Command = (QQCommand) reader.BEReadUInt16();
            Sequence = reader.BEReadChar();
            QQ = reader.BEReadInt32();
            reader.ReadBytes(3);
        }

        public long QQ { get; set; }
    }
}