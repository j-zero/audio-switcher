﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Media;
using System.Windows.Forms;
using AudioSwitcher.ComponentModel;
using AudioSwitcher.Presentation.CommandModel;

namespace AudioSwitcher.Presentation.UI
{
    // Responsible for sync'ing between a Command and a ToolStripMenuItem
    internal class ToolStripItemCommandBinding
    {
        private readonly ToolStripItem _item;
        private readonly ToolStripMenuItem _menuItem;
        private readonly ToolStripDropDown _dropDown;
        private readonly Lifetime<ICommand> _lifetime;
        private readonly ICommand _command;
        private readonly object _argument;

        public ToolStripItemCommandBinding(ToolStripDropDown dropDown, ToolStripItem item, Lifetime<ICommand> command, object argument)
        {
            if (dropDown == null)
                throw new ArgumentNullException("dropDown");

            if (item == null)
                throw new ArgumentNullException("item");

            if (command == null)
                throw new ArgumentNullException("command");

            _dropDown = dropDown;
            _item = item;
            _menuItem = item as ToolStripMenuItem;
            _command = command.Instance;
            _lifetime = command;
            _argument = argument;

            RegisterEvents();
            Refresh();
        }

        public object Argument
        {
            get { return _argument; }
        }

        private void RegisterEvents(bool register = true)
        {
            if (register)
            {
                _dropDown.Opening += OnContextMenuStripOpening;
                _dropDown.ItemRemoved += OnItemRemoved;
                _dropDown.ItemClicked += OnItemClicked;
                _command.PropertyChanged += OnCommandPropertyChanged;
            }
            else
            {
                _dropDown.Opening -= OnContextMenuStripOpening;
                _dropDown.ItemRemoved -= OnItemRemoved;
                _dropDown.ItemClicked -= OnItemClicked;
                _command.PropertyChanged -= OnCommandPropertyChanged;
            }
        }

        private void OnItemRemoved(object sender, ToolStripItemEventArgs e)
        {
            if (e.Item != _item)
                return;

            RegisterEvents(register: false);
            _lifetime.Dispose();
        }

        private void OnItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem != _item)
                return;

            if (!_command.IsInvokable)
            {
                SystemSounds.Beep.Play();
            }
            else
            {
                _command.Run(_argument);
            }
        }

        public void Refresh()
        {
            _command.Refresh(_argument);
            SyncProperty(_command, CommandProperty.IsInvokable);
            SyncProperty(_command, CommandProperty.IsVisible);
            SyncProperty(_command, CommandProperty.IsEnabled);
            SyncProperty(_command, CommandProperty.IsChecked);
            SyncProperty(_command, CommandProperty.Text);
            SyncProperty(_command, CommandProperty.Image);
            SyncProperty(_command, CommandProperty.TooltipText);
        }

        private void OnContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            Refresh();
        }

        private void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var command = (Command)sender;

            var propertyName = (CommandProperty)Enum.Parse(typeof(CommandProperty), e.PropertyName);

            SyncProperty(command, propertyName);
        }

        private void SyncProperty(ICommand command, CommandProperty propertyName)
        {
            if (propertyName == CommandProperty.IsVisible)
            {
                _item.Visible = command.IsVisible;
            }
            else if (propertyName == CommandProperty.IsEnabled)
            {
                _item.Enabled = command.IsEnabled;
            }
            else if (propertyName == CommandProperty.IsChecked)
            {
                if (_menuItem != null)
                {
                    _menuItem.Checked = command.IsChecked;
                }
            }
            else if (propertyName == CommandProperty.Text)
            {
                _item.Text = command.Text;
            }
            else if (propertyName == CommandProperty.TooltipText)
            {
                _item.ToolTipText = command.TooltipText;
            }
            else if (propertyName == CommandProperty.IsInvokable)
            {
                if (_item is AudioToolStripMenuItem item)
                {
                    item.AutoCloseOnClick = command.IsInvokable;
                }
            }
        }
    }
}
