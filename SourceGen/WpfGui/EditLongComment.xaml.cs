﻿/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Long comment editor.
    /// </summary>
    public partial class EditLongComment : Window, INotifyPropertyChanged {
        /// <summary>
        /// Get or set the multi-line comment object.  On exit, will be set to null if
        /// the user wants to delete the comment.
        /// </summary>
        public MultiLineComment LongComment { get; set; }

        private Asm65.Formatter mFormatter;

        public bool RenderInBox {
            get { return mRenderInBox; }
            set {
                mRenderInBox = value;
                OnPropertyChanged();
                FormatInput();
            }
        }
        private bool mRenderInBox;

        /// <summary>
        /// Text input by user.  This is bound to the input TextBox.
        /// </summary>
        public string UserInput {
            get { return mUserInput; }
            set {
                mUserInput = value;
                OnPropertyChanged();
                FormatInput();
            }
        }
        private string mUserInput;

        /// <summary>
        /// Generated text output.  This is bound to the output TextBox.
        /// </summary>
        public string TextOutput {
            get { return mTextOutput; }
            set {
                mTextOutput = value;
                OnPropertyChanged();
            }
        }
        private string mTextOutput;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ItemsSource for line width combo box.
        /// </summary>
        public int[] LineWidthItems { get; } = new int[] { 30, 40, 64, 80 };


        public EditLongComment(Window owner, Asm65.Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mFormatter = formatter;
            LongComment = new MultiLineComment(string.Empty);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Debug.Assert(LongComment != null);
            UserInput = LongComment.Text;
            RenderInBox = LongComment.BoxMode;

            // Try to find a match for the max width specified in the MLC.
            maxWidthComboBox.SelectedIndex = 0;
            for (int i = 0; i < maxWidthComboBox.Items.Count; i++) {
                if ((int)maxWidthComboBox.Items[i] == LongComment.MaxWidth) {
                    maxWidthComboBox.SelectedIndex = i;
                    break;
                }
            }

            FormatInput();
            entryTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        // Handle Ctrl+Enter as a way to close the dialog, since plain Enter just
        // moves to a new line.
        private void CloseCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            DialogResult = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (string.IsNullOrEmpty(UserInput)) {
                LongComment = null;
            } else {
                LongComment = CreateMLC();
            }
        }

        private void MaxWidthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            FormatInput();
        }

        /// <summary>
        /// Formats entryTextBox.Text into displayTextBox.Text.
        /// </summary>
        private void FormatInput() {
            if (!IsLoaded) {
                return;
            }

            MultiLineComment mlc = CreateMLC();
            if (mlc == null) {
                return;
            }
            List<string> lines = mlc.FormatText(mFormatter, string.Empty);

            StringBuilder sb = new StringBuilder(UserInput.Length + lines.Count * 2);
            //sb.AppendFormat("### got {0} lines\r\n", lines.Count);
            bool first = true;
            foreach (string line in lines) {
                if (first) {
                    first = false;
                } else {
                    sb.Append("\r\n");
                }
                sb.Append(line);
            }

            TextOutput = sb.ToString();
        }

        /// <summary>
        /// Creates a MultiLineComment from the current state of the dialog.
        /// </summary>
        /// <returns>New MultiLineComment object.  Returns null if the dialog is still
        ///   in the process of initializing.</returns>
        private MultiLineComment CreateMLC() {
            if (maxWidthComboBox.SelectedItem == null) {
                return null;    // still initializing
            }
            return new MultiLineComment(UserInput, RenderInBox,
                (int)maxWidthComboBox.SelectedItem);
        }
    }
}
