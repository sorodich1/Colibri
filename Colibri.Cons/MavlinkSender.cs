using System;
using System.ComponentModel;
using System.Net.Sockets;

namespace Colibri.Cons
{
    public class MavlinkSender(string ip, int port)
    {
        private UdpClient udpClient = new();
        private string targetIp = ip;
        private int targetPort = port;

        public void SendArmCommand()
        {
            // Создайте MAVLink сообщение команду ARM
            byte[] mavlinkMessage = CreateMavlinkArmMessage();

            // Отправьте сообщение
            udpClient.Send(mavlinkMessage, mavlinkMessage.Length, targetIp, targetPort);
        }

        private byte[] CreateMavlinkArmMessage(uint systemId = 1, uint componentId = 1)
        {
            // MAVLink сообщение типа COMMAND_LONG для ARM
            // Структура MAVLink 1.0 (пример)
            // Заголовок (синхронизатор, длина, seq, sysid, compid, msgid)
            // Payload (параметры)
            // CRC

            // Для простоты — пример с фиксированными значениями
            byte sync = 0xFE; // Start byte
            byte length = 33;  // длина полезных данных (payload)
            byte seq = 0;      // sequence, можно увеличить
            byte sysid = 1;    // системный ID
            byte compid = 1;   // компонент
            ushort msgid = 76; // COMMAND_LONG

            // Payload (33 байта)
            byte[] payload = new byte[33];
            // Заполняем payload:
            // target_system = 1
            // target_component = 1
            // command = 400 (ARM)
            // confirmation = 0
            // param1 = 1 (ARM)
            // остальные параметры 0
            payload[0] = 1; // target_system
            payload[1] = 1; // target_component
            BitConverter.GetBytes(msgid).CopyTo(payload, 2); // id
            payload[4] = 0; // confirmation
            payload[5] = 1; // param1 — армировать
                            // остальные параметры 0

            // Собираем весь пакет
            int packetSize = 6 + payload.Length + 2; // заголовок + payload + CRC
            byte[] packet = new byte[packetSize];

            packet[0] = sync;
            packet[1] = length;
            packet[2] = seq;
            packet[3] = sysid;
            packet[4] = compid;
            BitConverter.GetBytes(msgid).CopyTo(packet, 5);
            Array.Copy(payload, 0, packet, 7, payload.Length);

            // Расчет CRC (например, CRC-16/MCRF4XX)
            ushort crc = CalculateCRC(packet, 1, payload.Length + 6);
            BitConverter.GetBytes(crc).CopyTo(packet, packetSize - 2);

            return packet;
        }

        static ushort CalculateCRC(byte[] buffer, int start, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = start; i < start + length; i++)
            {
                crc ^= buffer[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 1) != 0)
                        crc = (ushort)((crc >> 1) ^ 0x1021);
                    else
                        crc >>= 1;
                }
            }
            return crc;
        }
    }
}
