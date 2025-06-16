using NModbus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DipMod.Model
{
    internal class Modbus
    {
        //представляет клиентское подключение по tcp
        readonly TcpClient client;
        //создает фабрику для работы стандартных функций
        readonly ModbusFactory factory = new ModbusFactory();
        //создаем мастера для работы
        readonly IModbusMaster master;
        //экземпляр класса для работы с sqlite
        public SQlite db;
        //началный адрес
        public ushort startAddress = 0;
        //количество считываемых регистров 
        public ushort numRegistersAnalog = 8;
        //количество цифровых регистров
        public ushort numRegistersADigital = 2;
        //конфигурация канала аналоговых портов
        readonly ushort channelConfig = 0x0008;
        private int znach = 10;
        readonly private byte mash;
        public Modbus(string IpAdress, int machine) {
            //инициализация tcp клиента для подключения
            client= new TcpClient(IpAdress, 502);
            //создаем мастера используя подключенного клиента
            master = factory.CreateMaster(client);
            mash = Convert.ToByte(machine);
            //отправляем на регистры конфигурацию параметров
            master.WriteSingleRegister(mash, 40001, channelConfig);
            master.WriteSingleRegister(mash, 40002, channelConfig);
            master.WriteSingleRegister(mash, 40003, channelConfig);
            master.WriteSingleRegister(mash, 40004, channelConfig);
            master.WriteSingleRegister(mash, 40005, channelConfig);
            master.WriteSingleRegister(mash, 40006, channelConfig);
            master.WriteSingleRegister(mash, 40007, channelConfig);
            master.WriteSingleRegister(mash, 40008, channelConfig);
            //инициализируем базу данных
            db = new SQlite("Port.db");
        }
        public void Disconnect()
        {
            //разрыв соединения
            client.Close();
        }
        int GetAnalog()
        {
            //считывание времени для записи в базу данных
            DateTime real = DateTime.Now;
            //считываем с устройства под указаным адресом аналоговые регистры начиная с адреса записаного в startAddress и количество numRegistersAnalog записывая это все в буфер analog
            ushort[] analog= master.ReadHoldingRegisters(mash,startAddress,numRegistersAnalog);
            // записывем стартовый адрес для записи в бд
            ushort countreg = startAddress;
            //перебираем буфер для записи
            foreach (ushort s in analog)
            {
                //добавляем в базу countreg который несет адрес значение регистра s и время взятия данных
                db.InsertAnalog(new AnalogPort(countreg,s,real));
                //увеличиваем значение регистра для обработки
                countreg++;
            }
            //возвращаем длину полученного сообщения
            return analog.Length;
        }
        //сбор данных напрямую без записи в бд
        public ObservableCollection<AnalogPort> GetAnalogOnline()
        {
            try
            {
                //считывание времени
                DateTime real = DateTime.Now;
                //считываем с устройства под указаным адресом аналоговые регистры начиная с адреса записаного в startAddress и количество numRegistersAnalog записывая это все в буфер analog
                ushort[] analog = master.ReadHoldingRegisters(mash, startAddress, numRegistersAnalog);
                // записывем стартовый адрес для записи в бд
                ushort countreg = startAddress;
                //создаем колекцию для последующего вывода на график
                ObservableCollection<AnalogPort> Aports = new ObservableCollection<AnalogPort>();
                //перебираем буфер для записи
                foreach (float s in analog)
                {
                    //преобразуем абсолютные значения в вольты
                    float val = s / 65536 * (znach - (-znach)) + (-znach);
                    //записываем в коллекцию
                    countreg++;
                    Aports.Add(new AnalogPort(countreg, val, real));
                    //увеличиваем значение регистра для обработки
                    
                }
                // возвращаем коллекцию
                return Aports;
            }
            // обработка ошибок
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                client.Close();
                return null;
            }
        }
        //метод сбора цифровых данных
        int GetDigital()
        {
            //считывание времени для записи в базу данных
            DateTime real = DateTime.Now;
            //считываем с устройства под указаным адресом цифровые регистры начиная с адреса записаного в startAddress и количество numRegistersADigital записывая это все в буфер digitalOutputs 
            bool[] digitalOutputs = master.ReadInputs(mash, startAddress, numRegistersADigital);
            // записывем стартовый адрес для записи в бд
            ushort countreg = startAddress;
            //перебираем полученный список для записи в бд
            foreach (bool s in digitalOutputs)
            {
                //добавляем в базу countreg который несет адрес значение регистра s и время взятия данных
                db.InsertDigital(new DigitalPort(countreg,s,real));
                //увеличиваем значение регистра для обработки
                countreg++;
            }
            //возвращаем 0
            return 0;
        }
        //функция начала сбора данных аналоговых и цифровых
        public void Start()
        {
            try
            {
                GetAnalog();
                GetDigital();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                client.Close();
            }
        }
    }
}
