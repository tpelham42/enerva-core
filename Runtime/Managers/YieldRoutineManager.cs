using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EnervaCore.YieldRoutines;

namespace EnervaCore {
    public class YieldRoutineManager : IManager, IManagerUpdate {
        List<YieldRoutine> _routineMethods;
        List<YieldRoutine> _rawUpdateRoutineMethods;
        List<YieldRoutine> _removeList;


        public YieldRoutineManager() {
            _routineMethods = new List<YieldRoutine>();
            _removeList = new List<YieldRoutine>();
            _rawUpdateRoutineMethods = new List<YieldRoutine>();
        }

        public void RegisterYieldRoutine(YieldRoutine r) {
            if (r == null)
                return;

            if (r.UseRawTime) {
                if (_rawUpdateRoutineMethods.Contains(r) == false) {
                    _rawUpdateRoutineMethods.Add(r);
                }
            }
            else {
                if (_routineMethods.Contains(r) == false) {
                    _routineMethods.Add(r);
                }
            }
        }

        public void RemoveYieldRoutine(YieldRoutine r) {
            if (_routineMethods.Contains(r))
                _routineMethods.Remove(r);

            if (_rawUpdateRoutineMethods.Contains(r))
                _rawUpdateRoutineMethods.Remove(r);
        }

        public void Update(float deltaTime) {
            UpdateList(_routineMethods);
        }

        public void UpdateRaw(float deltaTime) {
            UpdateList(_rawUpdateRoutineMethods);
        }

        void UpdateList(List<YieldRoutine> routineList) {
            //Yield Method Updates
            for (int i = 0; i < routineList.Count(); i++) {
                YieldRoutine yr = routineList[i];
                try {
                    yr.Update();
                }
                catch (Exception e) {
                    Debug.LogError(string.Format("{0} :: Exception Occured While Updating Yield Routine. Exception Details: {1}", this, e.ToString()));
                    yr.HasErrorException = true;
                    _removeList.Add(yr);
                    continue;
                }

                if (yr.Completed()) {
                    _removeList.Add(yr);
                }
            }

            //Remove List Handling
            while (_removeList.Count > 0) {
                YieldRoutine yr = _removeList[0];
                routineList.Remove(yr);
                _removeList.RemoveAt(0);

                if (yr.HasErrorException) {
                    yr.HandleException();
                }

                yr.Dispose();
            }
        }

        public void Initialize() {}

        public void OnDestroyed() {
            if (_routineMethods != null) {
                foreach (YieldRoutine yr in _routineMethods) {
                    if (yr != null) {
                        yr.Cancel();
                    }
                }

                _routineMethods.Clear();
                _routineMethods = null;
            }
        }
    }
}