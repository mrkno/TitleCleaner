﻿#region

using System;
using System.Windows.Forms;
using MediaFileParser.MediaTypes.MediaFile;
using MediaFileParser.MediaTypes.MovieFile;
using MediaFileParser.MediaTypes.TvFile;

#endregion

namespace TitleCleanerGui
{
    public partial class Settings : Form
    {
        private bool _confirm;
        private bool _copy;
        private int _fileType;
        private string _input;
        private int _mode;
        private string _output;

        public Settings()
        {
            InitializeComponent();
            comboBoxMode.SelectedIndex = 0;
            comboBoxFileType.SelectedIndex = 0;
            textBoxCommonName.Text = MediaFile.DefaultFormatString;
            textBoxTvName.Text = TvFile.DefaultFormatString;
            textBoxMovieName.Text = MovieFile.DefaultFormatString;
            textBoxTvFolder.Text = TvFile.TypeDirectory;
            textBoxMovieFolder.Text = MovieFile.TypeDirectory;
        }

        private void ButtonDirectoryClick(object sender, EventArgs e)
        {
            var res = "None";
            switch (comboBoxMode.SelectedIndex)
            {
                case 0:
                case 1:
                {
                    var directoryDialog = new FolderBrowserDialog
                                          {
                                              Description = "Select directory to output files to.",
                                              ShowNewFolderButton = false
                                          };

                    if (directoryDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    res = directoryDialog.SelectedPath;
                    break;
                }
                    /*case 1:
                {
                    var openFileDialog = new OpenFileDialog
                    {
                        CheckPathExists = true,
                        CheckFileExists = true,
                        AutoUpgradeEnabled = true,
                        Multiselect = false,
                        Title = "Select a media file to clean.",
                        Filter =
                            "*.mov|*.mov|*.mkv|*.mkv|*.flv|*.flv|*.avi|*.avi|*.mp4|*.mp4|*.mpg|*.mpg|*.vob|*.vob|*.m4v|*.m4v|*.mpeg|*.mpeg|*.ogg|*.ogg|*.swf|*.swf|*.wmv|*.wmv|*.wtv|*.wtv|*.h264|*.h264"
                    };

                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    res = openFileDialog.FileName;
                    break;
                }*/
                case 2:
                {
                    var openFileDialog = new SaveFileDialog
                                         {
                                             CheckPathExists = true,
                                             CheckFileExists = true,
                                             AutoUpgradeEnabled = true,
                                             Title = "Select output test file.",
                                             Filter = "*.csv|*.csv"
                                         };

                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    res = openFileDialog.FileName;
                    break;
                }
            }
            textBoxOutputDir.Text = res;
            textBoxTvFolder.Enabled = true;
            textBoxMovieFolder.Enabled = true;
        }

        private void TextBoxCommonNameTextChanged(object sender, EventArgs e)
        {
            MediaFile.DefaultFormatString = textBoxCommonName.Text;
            textBoxTvName.Text = TvFile.DefaultFormatString;
            textBoxMovieName.Text = MovieFile.DefaultFormatString;
        }

        private void TextBoxTvNameTextChanged(object sender, EventArgs e)
        {
            if (MediaFile.DefaultFormatString != textBoxTvName.Text)
            {
                TvFile.DefaultFormatString = textBoxTvName.Text;
            }
        }

        private void TextBoxMovieNameTextChanged(object sender, EventArgs e)
        {
            if (MediaFile.DefaultFormatString != textBoxTvName.Text)
            {
                MovieFile.DefaultFormatString = textBoxMovieName.Text;
            }
        }

        private void ComboBoxModeSelectedIndexChanged(object sender, EventArgs e)
        {
            checkBoxCopy.Enabled = true;
            switch (comboBoxMode.SelectedIndex)
            {
                case 0:
                {
                    labelInputDir.Text = "Input Directory";
                    labelInputDirDesc.Text = "Directory to read files from.";
                    labelOutput.Text = "Output Directory";
                    labelDirFiles.Text = "Directory to move files to.";
                    textBoxInputDir.Text = "Current Directory";
                    break;
                }
                case 1:
                {
                    labelInputDir.Text = "Input File";
                    labelInputDirDesc.Text = "File to clean the name of.";
                    labelOutput.Text = "Output Directory";
                    labelDirFiles.Text = "Directory to move files to.";
                    textBoxInputDir.Text = "None";
                    break;
                }
                case 2:
                {
                    labelInputDir.Text = "Tests File";
                    labelInputDirDesc.Text = "File containing the tests to be run.";
                    labelOutput.Text = "Test CSV Output";
                    labelDirFiles.Text = "Location to output a CSV file summarising the tests.";
                    textBoxInputDir.Text = "None";
                    checkBoxCopy.Enabled = false;
                    break;
                }
            }
            textBoxOutputDir.Text = "None";
            textBoxTvFolder.Enabled = false;
            textBoxMovieFolder.Enabled = false;
        }

        private void ButtonInputDirClick(object sender, EventArgs e)
        {
            var res = "Current Directory";
            switch (comboBoxMode.SelectedIndex)
            {
                case 0:
                {
                    var directoryDialog = new FolderBrowserDialog
                                          {
                                              Description = "Select directory to clean files in.",
                                              ShowNewFolderButton = false
                                          };

                    if (directoryDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    res = directoryDialog.SelectedPath;
                    break;
                }
                case 1:
                {
                    var openFileDialog = new OpenFileDialog
                                         {
                                             CheckPathExists = true,
                                             CheckFileExists = true,
                                             AutoUpgradeEnabled = true,
                                             Multiselect = false,
                                             Title = "Select a media file to clean.",
                                             Filter =
                                                 "*.mov|*.mov|*.mkv|*.mkv|*.flv|*.flv|*.avi|*.avi|*.mp4|*.mp4|*.mpg|*.mpg|*.vob|*.vob|*.m4v|*.m4v|*.mpeg|*.mpeg|*.ogg|*.ogg|*.swf|*.swf|*.wmv|*.wmv|*.wtv|*.wtv|*.h264|*.h264"
                                         };

                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    res = openFileDialog.FileName;
                    break;
                }
                case 2:
                {
                    var openFileDialog = new OpenFileDialog
                                         {
                                             CheckPathExists = true,
                                             CheckFileExists = true,
                                             AutoUpgradeEnabled = true,
                                             Multiselect = false,
                                             Title = "Select test file.",
                                             Filter = "*.csv|*.csv"
                                         };

                    if (openFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    res = openFileDialog.FileName;
                    break;
                }
            }
            textBoxInputDir.Text = res;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            _confirm = checkBoxConfirmations.Checked;
            _input = textBoxInputDir.Text;
            _output = textBoxOutputDir.Text;
            _fileType = comboBoxFileType.SelectedIndex;
            _mode = comboBoxMode.SelectedIndex;
            _copy = checkBoxCopy.Checked;

            if ((_mode == 1 || _mode == 2) && _input == "None")
            {
                MessageBox.Show("An input file must be specified in this mode.", "Title Cleaner", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }

        private void CheckBoxTvdbCheckedChanged(object sender, EventArgs e)
        {
            TvFile.TvdbLookup = checkBoxTvdb.Checked;
        }

        private void CheckBoxConfirmationsCheckedChanged(object sender, EventArgs e)
        {
            TvFile.TvdbLookupConfirm = checkBoxConfirmations.Checked;
        }

        private void ButtonCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        public bool GetConfirm()
        {
            return _confirm;
        }

        public string GetInput()
        {
            return _input == "Current Directory" || _input == "None" ? Environment.CurrentDirectory : _input;
        }

        public string GetOuput()
        {
            return _output == "None" ? "" : _output;
        }

        public Type GetMediaType()
        {
            switch (_fileType)
            {
                case 0:
                    return null;
                case 1:
                    return typeof (TvFile);
                case 2:
                    return typeof (MovieFile);
                default:
                    goto case 0;
            }
        }

        public int GetMode()
        {
            return _mode;
        }

        private void TextBoxTvFolderTextChanged(object sender, EventArgs e)
        {
            TvFile.TypeDirectory = textBoxTvFolder.Text;
        }

        private void TextBoxMovieFolderTextChanged(object sender, EventArgs e)
        {
            MovieFile.TypeDirectory = textBoxMovieFolder.Text;
        }

        private void ComboBoxFileTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxTv.Enabled = true;
            groupBoxMovie.Enabled = true;
            switch (comboBoxFileType.SelectedIndex)
            {
                case 1:
                    groupBoxMovie.Enabled = false;
                    break;
                case 2:
                    groupBoxTv.Enabled = false;
                    break;
            }
        }

        public bool GetCopy()
        {
            return _copy;
        }
    }
}