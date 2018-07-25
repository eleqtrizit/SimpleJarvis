using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Speech.Synthesis;
using System.Speech.Recognition;

namespace SomebodyNeedsMe
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		// Grammar and Responses
		Dictionary<string, string> responses = new Dictionary<string, string>();
		List<ResponsePair> responseList = new List<ResponsePair>();
		Choices grammarList = new Choices();

		// speech synth
		SpeechSynthesizer speechSynth = new SpeechSynthesizer();

		// speech recog
		SpeechRecognitionEngine speechRecognition = new SpeechRecognitionEngine();


		public MainWindow()
		{
			LoadDataFile();
			Grammar grammar = new Grammar(new GrammarBuilder(grammarList));

			try
			{
				speechRecognition.RequestRecognizerUpdate();
				speechRecognition.LoadGrammar(grammar);
				speechRecognition.SpeechRecognized += Respond;
				speechRecognition.SetInputToDefaultAudioDevice();
				speechRecognition.RecognizeAsync(RecognizeMode.Multiple);
			}
			catch
			{
				// we are not handling
				return;
			}
			speechSynth.SelectVoiceByHints(VoiceGender.Female);
			InitializeComponent();
		}



		private void Respond(object sender, SpeechRecognizedEventArgs e)
		{
			string weHeard = e.Result.Text.ToLower();
			Console.WriteLine("<" + weHeard + ">");

			Console.WriteLine("Just heard: " + weHeard);

			var response = from a in responseList
						   where a.Hear.ToLower() == weHeard
						   select a.Say;

			speechSynth.SpeakAsync(response.First());
		}

		public void LoadDataFile()
		{
			// file store
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SomebodyNeedsMe";
			string filePath = folderPath + @"\call and responses.txt";
			Directory.CreateDirectory(folderPath); // check isn't needed
			if (!File.Exists(filePath))
			{
				File.CreateText(filePath);
				File.OpenRead(filePath);
			}


			String[] dataFile = (File.ReadAllLines(filePath));

			if (dataFile.Count() > 0)
			{
				foreach (var line in dataFile)
				{
					string[] words = line.Split(':');
					//Console.WriteLine("<" + words[0].Trim() + "->" + words[1].Trim() + ">");
					grammarList.Add(words[0].Trim().ToLower());
					ResponsePair pair = new ResponsePair
					{
						Hear = words[0].Trim(),
						Say = words[1].Trim()
					};
					responseList.Add(pair);
				}
			}
			else
			{
				// load some defaults
				grammarList.Add("Hello");
				ResponsePair pair = new ResponsePair
				{
					Hear = "Hello",
					Say = "Hi. How are you?"
				};
				responseList.Add(pair);
			}

		}

		private void CallsAndResponses_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void CallsAndResponses_Loaded(object sender, RoutedEventArgs e)
		{
			var grid = sender as DataGrid;
			grid.ItemsSource = responseList;
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SomebodyNeedsMe";
			string filePath = folderPath + @"\call and responses.txt";
			Console.WriteLine(responseList[1].Say);
			List<string> writeThis = new List<string>();
			foreach (var item in responseList)
			{
				writeThis.Add(item.Hear.Trim() + ":" + item.Say.Trim());
			}
			File.WriteAllLines(filePath, writeThis);
			System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
			Application.Current.Shutdown();
		}
	}

	public class ResponsePair
	{
		public string Hear { get; set; }
		public string Say { get; set; }
	}

}
