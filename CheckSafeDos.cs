using Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Magicnet.CheckSafeDos
{
    public class CheckSafeDos
    {
        private ConcurrentDictionary<long, User> _dCheckDos = new ConcurrentDictionary<long, User>();

        private int _max_cnt;
        private int _delta_minutes;

        public CheckSafeDos(int max_cnt, int delta_minutes)
        {
            _max_cnt = max_cnt;
            _delta_minutes = delta_minutes;
        }

        public bool checkUserId(long chatId)
        {
            User user;

            if (!_dCheckDos.TryGetValue(chatId, out user))
            {
                _restartUser(chatId);

                return true;
            }
            else
            {
                if ((DateTime.Now - user.last).TotalMinutes >= _delta_minutes)
                {
                    _restartUser(chatId);

                    return true;
                }
                else
                {
                    if (user.cnt < _max_cnt)
                    {
                        user.cnt++;
                        _dCheckDos.AddOrUpdate(chatId, user, (key, oldValue) => user);
                        return true;
                    }
                    else return false;
                }
            }

            return false;
        }

        private void _restartUser(long Id)
        {
            User user = new User()
            {
                cnt = 1,
                first = DateTime.Now,
                last = DateTime.Now,
                Id = Id,
            };

            _dCheckDos.AddOrUpdate(Id, user, (key, oldValue) => user);
        }

        public void updateParamMaxCnt(int maxcnt)
        {
            _max_cnt = maxcnt;            

            _dCheckDos.Clear();

            TLog.ToLogAsync("new MAX_CNT " + maxcnt.ToString());
        }

        public void updateParamDeltaMinutes(int delta)
        {
            _delta_minutes = delta;

            _dCheckDos.Clear();

            TLog.ToLogAsync("new DELTA_MINUTES " + delta.ToString());
        }
    }
}
