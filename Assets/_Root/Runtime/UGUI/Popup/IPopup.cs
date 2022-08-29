using System.Threading;
using UnityEngine;

namespace Pancake.UI
{
    public interface IPopup
    {
        /// <summary>
        /// type name
        /// </summary>
        string Id { get; }

        /// <summary>
        /// gameobject
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// same as gameObject.activeSelf
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// back button pressed.
        /// </summary>
        bool BackButtonPressed { get; }

        /// <summary>
        /// canvas contains popup
        /// </summary>
        Canvas Canvas { get; }

        /// <summary>
        /// active popup
        /// </summary>
        void Show(CancellationToken token = default);

        /// <summary>
        /// deactive popup
        /// </summary>
        void Close();

        /// <summary>
        /// update sorting order of cavas contains popup
        /// </summary>
        /// <param name="sortingOrder"></param>
        void UpdateSortingOrder(int sortingOrder);

        /// <summary>
        /// refresh popup
        /// </summary>
        void Refresh();

        /// <summary>
        /// 
        /// </summary>
        void ActivePopup();

        /// <summary>
        /// 
        /// </summary>
        void DeActivePopup();

        /// <summary>
        /// rise popup in top of stack
        /// </summary>
        void Rise();

        /// <summary>
        /// collapse 'current' popup when have new popup rise (in case current popup not hide)
        /// </summary>
        void Collapse();
    }
}