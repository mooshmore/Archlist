using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace PlaylistSaver.Resources.Behaviours
{
    //https://stackoverflow.com/questions/555252/show-contextmenu-on-left-click-using-only-xaml/29123964#29123964
    public static class LeftClickContextMenu
    {
        public static bool GetIsLeftClickEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLeftClickEnabledProperty);
        }

        public static void SetIsLeftClickEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLeftClickEnabledProperty, value);
        }

        public static readonly DependencyProperty IsLeftClickEnabledProperty = DependencyProperty.RegisterAttached(
            "IsLeftClickEnabled",
            typeof(bool),
            typeof(LeftClickContextMenu),
            new UIPropertyMetadata(false, OnIsLeftClickEnabledChanged));

        private static void OnIsLeftClickEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement uiElement)
            {
                bool IsEnabled = e.NewValue is bool boolean && boolean;

                if (IsEnabled)
                {
                    if (uiElement is ButtonBase @base)
                        @base.Click += OnMouseLeftButtonUp;
                    else
                        uiElement.MouseLeftButtonUp += OnMouseLeftButtonUp;
                }
                else
                {
                    if (uiElement is ButtonBase @base)
                        @base.Click -= OnMouseLeftButtonUp;
                    else
                        uiElement.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                }
            }
        }

        public static bool GetBindToTag(DependencyObject obj)
        {
            return (bool)obj.GetValue(BindToTagProperty);
        }

        public static void SetBindToTag(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLeftClickEnabledProperty, value);
        }

        public static readonly DependencyProperty BindToTagProperty = DependencyProperty.RegisterAttached(
            "BindToTag",
            typeof(bool),
            typeof(LeftClickContextMenu));

        private static void OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            Debug.Print("OnMouseLeftButtonUp");
            if (sender is FrameworkElement fe)
            {
                // if we use binding in our context menu, then it's DataContext won't be set when we show the menu on left click
                // (it seems setting DataContext for ContextMenu is hardcoded in WPF when user right clicks on a control, although I'm not sure)
                // so we have to set up ContextMenu.DataContext manually here
                if (fe.ContextMenu.DataContext == null)
                {
                    if ((bool)((FrameworkElement)sender).GetValue(BindToTagProperty))
                        fe.ContextMenu.SetBinding(FrameworkElement.DataContextProperty, new Binding { Source = fe.Tag });
                    else
                        fe.ContextMenu.SetBinding(FrameworkElement.DataContextProperty, new Binding { Source = fe.DataContext });
                }

                fe.ContextMenu.IsOpen = true;
            }
        }

    }
}
