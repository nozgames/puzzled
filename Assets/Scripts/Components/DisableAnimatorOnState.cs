namespace Puzzled
{
    using UnityEngine;

    namespace Puzzled
    {
        class DisableAnimatorOnState : StateMachineBehaviour
        {
            [SerializeField] private string _stateName;

            private int _stateHash;

            public void Awake()
            {
                _stateHash = Animator.StringToHash(_stateName);
            }

            override public void OnStateEnter(Animator animator,
                                              AnimatorStateInfo stateInfo,
                                              int layerIndex)
            {
                if (stateInfo.shortNameHash == _stateHash)
                    animator.enabled = false;
            }
        }
    }

}
