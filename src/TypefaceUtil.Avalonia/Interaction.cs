using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TypefaceUtil.Avalonia
{
    public abstract class Behavior
    {
        
    }

    public class Interaction
    {
        static Interaction()
        {
            var onClickHandlers = new Dictionary<Button, EventHandler<RoutedEventArgs>>();

            OnClickProperty.Changed.Subscribe(e =>
            {
                var oldAction = e.OldValue.GetValueOrDefault();
                var newAction = e.NewValue.GetValueOrDefault();

                if (oldAction == newAction)
                {
                    return;
                }

                if (e.Sender is Button button)
                {
                    if (oldAction is { } && onClickHandlers.ContainsKey(button))
                    {
                        var handler = onClickHandlers[button];
                        button.Click -= handler;
                    }

                    if (newAction is { })
                    {
                        void Handler(object? sender, RoutedEventArgs args) => newAction?.Invoke();
                        button.Click += Handler;
                        onClickHandlers.Add(button, Handler);
                    }
                }
            });
        }

        public static readonly AttachedProperty<Action> OnClickProperty = 
            AvaloniaProperty.RegisterAttached<Interaction, Button, Action>("OnClick");

        public static Action GetOnClick(Button button)
        {
            var action = button.GetValue(OnClickProperty);

            button.Click += (_, _) => action?.Invoke();

            return action;
        }

        public static void SetOnClick(Button button, Action action)
        {
            button.SetValue(OnClickProperty, action);
            
            
            button.Click += (_, _) => action?.Invoke();

        }
    }
}
