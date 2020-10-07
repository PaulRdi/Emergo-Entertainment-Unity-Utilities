using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.ComponentModel;

/// <summary>
/// MonoBehaviour which allows binding to any model class which implements System.ComponentModel.INotifyPropertyChanged.
/// Inherit from this class implement UI class for model class. 
/// 
/// </summary>
/// <typeparam name="T">The model class which should be bindable.</typeparam>
public abstract class BoundMonoBehaviour<T> : MonoBehaviour where T : class, INotifyPropertyChanged
{
    public T data { get; private set; }
    public void Bind(T data)
    {
        this.data = data;
        data.PropertyChanged += Data_PropertyChanged;
    }

    private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (!(sender is T))
            throw new System.InvalidCastException("The binding did not equal the binding class type.");

        PropertyChanged(e.PropertyName, sender as T);
    }

    protected abstract void PropertyChanged(string propertyName, T sender);
}
