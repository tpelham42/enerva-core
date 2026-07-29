using System;
using System.Collections;
using UnityEngine;

namespace EnervaCore.YieldRoutines {
    public class YieldRoutine {
        IEnumerator yieldMethod;


        public YieldTask WaitForTask;
        public bool _useRawTime = false;
        IEnumerator yieldTask;


        bool _isCompleted;
        bool _hasErrorException;

        YieldRoutineManager _yrMgr;

        public Action<YieldRoutine> CBHandleException;

        public YieldRoutine(IEnumerator routineMethod, bool useRawTime = false) {
            _isCompleted = false;
            yieldMethod = routineMethod;
            _useRawTime = useRawTime;

            _yrMgr = ECM.Main.GetManager<YieldRoutineManager>();
            _yrMgr.RegisterYieldRoutine(this);

        }

        public void HandleException() {
            if (CBHandleException != null) {
                CBHandleException(this);
            }
        }

        public virtual void Cancel() {
            Dispose();

            if (_yrMgr != null)
                _yrMgr.RemoveYieldRoutine(this);
        }

        public void Dispose() {
            _isCompleted = true;
            yieldMethod = null;
            yieldTask = null;
            WaitForTask = null;
            CBHandleException = null;
        }

        public virtual bool Completed() {
            return _isCompleted;
        }

        public bool HasErrorException {
            get { return _hasErrorException; }
            set { _hasErrorException = value; }
        }

        public bool UseRawTime {
            get { return _useRawTime; }
        }

        public void Update() {
            //If YieldTask then do that first
            if (WaitForTask != null) {
                if (yieldTask.MoveNext() == false) {
                    WaitForTask = null;
                    yieldTask = null;
                }

                return;
            }

            if (yieldMethod == null) {
                return;
            }

            //Move next on main method
            if (yieldMethod.MoveNext() == false) {
                _isCompleted = true;
            }

            if (yieldMethod != null && yieldMethod.Current != null) {
                WaitForTask = yieldMethod.Current as YieldTask;
                if (WaitForTask != null) {
                    yieldTask = WaitForTask.Run();
                }
            }

        }
    }

    public class YieldTask {
        public virtual IEnumerator Run() { return null; }
    }
}