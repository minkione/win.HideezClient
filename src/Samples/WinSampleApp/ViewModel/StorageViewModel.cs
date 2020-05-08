using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Dasync.Collections;
using Hideez.SDK.Communication.Interfaces;
using Hideez.SDK.Communication.Log;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Utils;

namespace WinSampleApp.ViewModel
{
    public class StorageRecordViewModel
    {
        public ushort Key { get; set; }
        public string StringData { get; set; }
        public string HexData { get; set; }
    }

    public class StorageViewModel : ViewModelBase
    {
        readonly EventLogger _log;
        int _counter = 1;

        public DeviceViewModel CurrentDevice { get; }
        public IDevice Device => CurrentDevice.Device;

        public byte Table { get; set; }
        public ObservableCollection<StorageRecordViewModel> Rows { get; set; } = new ObservableCollection<StorageRecordViewModel>();

        public StorageRecordViewModel SelectedRow
        {
            get
            {
                return null;
            }
            set
            {
                Key = value.Key;
            }
        }

        public ICollectionView MyRows { get; set; }


        ushort _key;
        public ushort Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    NotifyPropertyChanged(nameof(Key));
                }
            }
        }

        string _data;
        private DevicePasswordManager _pm;

        public string Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (_data != value)
                {
                    _data = value;
                    NotifyPropertyChanged(nameof(Data));
                }
            }
        }

        #region Commands
        public ICommand ReadRowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        ReadRow();
                    }
                };
            }
        }

        public ICommand ReadTableCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        ReadTable();
                    }
                };
            }
        }

        public ICommand WriteRowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return Device != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        WriteRow();
                    }
                };
            }
        }

        public ICommand WriteTableCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return Device != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        WriteTable();
                    }
                };
            }
        }

        public ICommand DeleteRowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        DeleteRow();
                    }
                };
            }
        }

        public ICommand ClearTableCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        ClearTable();
                    }
                };
            }
        }

        public ICommand WriteAccountCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        WriteAccount();
                    }
                };
            }
        }

        public ICommand LoadPasswordManagerCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () =>
                    {
                        return CurrentDevice != null && Device.IsConnected;
                    },
                    CommandAction = (x) =>
                    {
                        LoadPasswordManager();
                    }
                };
            }
        }
        #endregion

        public StorageViewModel(DeviceViewModel currentDevice, EventLogger log)
        {
            _log = log;
            Table = 34;
            Key = 1;
            Data = string.Empty;
            CurrentDevice = currentDevice;
           // MyRows = CollectionViewSource.GetDefaultView(Rows);
        }

        async void ReadRow()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var readResult = await Device.ReadStorageAsString(Table, Key);

                //for (byte i = 1; i < 255; i++)
                //{
                //    var ddd = await Device.ReadStorageAsString(i, 1);
                //    if (ddd != null)
                //    {
                //        Debug.WriteLine($"Storage: table {i}, row 1: {ddd}");
                //    }
                //}

                Data = readResult;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void ReadTable()
        {
            try
            {
                Rows.Clear();

                Mouse.OverrideCursor = Cursors.Wait;
                var sw = new Stopwatch();
                sw.Start();

                var readResult = Device.EnumRecordsAsync(Table);

                await readResult.ForEachAsync(t =>
                {
                    var vm = new StorageRecordViewModel()
                    {
                        Key = t.Key,
                        StringData = t.StringData,
                        HexData = ConvertUtils.ByteArrayToString(t.Data) 
                    };

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Rows.Add(vm);
                    });
                });

                sw.Stop();
                MessageBox.Show($"Elapsed: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void WriteRow()
        {
            try
            {
                if (Data == null)
                    throw new Exception("Data cannot be empty");

                Mouse.OverrideCursor = Cursors.Wait;
                var newKey = await Device.WriteStorage(Table, Key, Data);
                Key = newKey;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void WriteTable()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var sw = new Stopwatch();
                sw.Start();

                for (ushort i = 1; i <= 100; i++)
                {
                    var newKey = await Device.WriteStorage(Table, i, $"{i} - 1234567890qwertyuiopasdfghjklzxcvbnm");
                }

                sw.Stop();
                MessageBox.Show($"Elapsed: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void DeleteRow()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await Device.DeleteStorage(Table, Key);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void ClearTable()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var sw = new Stopwatch();
                sw.Start();

                foreach (var item in Rows)
                {
                    await Device.DeleteStorage(Table, item.Key);
                }

                sw.Stop();
                MessageBox.Show($"Elapsed: {sw.Elapsed}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void WriteAccount()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var pm = new DevicePasswordManager(Device, _log);

                // array of records
                //for (int i = 0; i < 100; i++)
                //{
                //    var account = new AccountRecord()
                //    {
                //        Key = 0,
                //        Name = $"My Google Account {i}",
                //        Login = $"admin_{i}@hideez.com",
                //        Password = $"my_password_{i}",
                //        OtpSecret = $"asdasd_{i}",
                //        Apps = $"12431412412342134_{i}",
                //        Urls = $"www.hideez.com;www.google.com_{i}",
                //        IsPrimary = i == 0
                //    };

                //    var key = await pm.SaveOrUpdateAccount(account.Key, account.Flags, account.Name,
                //        account.Password, account.Login, account.OtpSecret,
                //        account.Apps, account.Urls,
                //        account.IsPrimary);

                //    Debug.WriteLine($"^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ Writing {i} account");
                //}


                // single record
                var account = new AccountRecord()
                {
                    StorageId = ConvertUtils.ConvertToUnixTime(DateTime.Now),
                    Timestamp = ConvertUtils.ConvertToUnixTime(DateTime.Now),
                    Name = $"{Data} My Google Account {_counter}",
                    Login = $"{Data} admin_0@hideez.com {_counter}",
                    Password = $"{Data} my_password_{_counter}",
                    OtpSecret = $"DPMYUOUOQDCAABSIAE5DFBANESXGOHDV",
                    Apps = $"{Data} 12431412412342134_{_counter}",
                    Urls = $"{Data} www.hideez.com;www.google.com_{_counter}",
                    IsPrimary = false
                };

                _counter++;

                await pm.SaveOrUpdateAccount(
                    account.StorageId, account.Timestamp,
                    account.Name,
                    account.Password, account.Login, account.OtpSecret,
                    account.Apps, account.Urls,
                    account.IsPrimary
                    //,(ushort)(StorageTableFlags.RESERVED7 | StorageTableFlags.RESERVED6) 
                    //,(ushort)(StorageTableFlags.RESERVED7 | StorageTableFlags.RESERVED6)
                    );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        async void LoadPasswordManager()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (_pm == null)
                    _pm = new DevicePasswordManager(Device, _log);
                await _pm.Load();

                foreach (var a in _pm.Accounts.Values)
                {
                    _log.WriteLine("PM", $"Account {a.Key},\t {a.Flags:X},\t {a.IsPrimary},\t {a.Name},\t {a.Login},\t {a.Apps},\t {a.Urls}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }



    }
}
