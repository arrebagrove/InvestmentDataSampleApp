﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;

using InvestmentDataSampleApp.Shared;

namespace InvestmentDataSampleApp
{
	public class OpportunitiesViewModel : BaseViewModel
	{
		#region Fields
		string _searchBarText;
		IList<OpportunityModel> _allOpportunitiesData, _viewableOpportunitiesData;
		Command _okButtonTapped;
		Command<string> _filterTextEnteredCommand;
		Command<bool> _refreshAllDataCommand;
		#endregion

		#region Constructors
		public OpportunitiesViewModel()
		{

			MessagingCenter.Subscribe<object>(this, "RefreshData", async (sender) =>
		   	{
				   await RefreshOpportunitiesDataAsync();
		   	});

			Task.Run(async () =>
			{
				// If the database is empty, initialize the database with dummy data
				if (await App.Database.GetNumberOfRowsAsync() < 20)
				{
					await InitializeDataInDatabaseAsync();
				}
				await RefreshOpportunitiesDataAsync();
			});
		}
		#endregion

		#region Events
		public event EventHandler OkButtonTappedEvent;
		public event EventHandler PullToRefreshDataCompleted;
		#endregion

		#region Properties
		public string SearchBarText
		{
			get { return _searchBarText; }
			set { SetProperty(ref _searchBarText, value, () => FilterList(value)); }
		}

		public IList<OpportunityModel> AllOpportunitiesData
		{
			get { return _allOpportunitiesData; }
			set { SetProperty(ref _allOpportunitiesData, value, () => FilterList(SearchBarText)); }
		}

		public IList<OpportunityModel> ViewableOpportunitiesData
		{
			get { return _viewableOpportunitiesData; }
			set { SetProperty(ref _viewableOpportunitiesData, value); }
		}

		public Command OkButtonTapped => _okButtonTapped ??
			(_okButtonTapped = new Command(ExecuteOkButtonTapped));

		public Command<string> FilterTextEnteredCommand => _filterTextEnteredCommand ??
			(_filterTextEnteredCommand = new Command<string>(ExecuteFilterTextEnteredCommand));

		public Command<bool> RefreshAllDataCommand => _refreshAllDataCommand ??
			(_refreshAllDataCommand = new Command<bool>(async b => await ExecuteRefreshAllDataCommand(b)));
		#endregion

		#region Methods
		public async Task RefreshOpportunitiesDataAsync()
		{
			AllOpportunitiesData = await App.Database.GetAllOpportunityDataAsync_OldestToNewest();
		}

		public void FilterList(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				ViewableOpportunitiesData = AllOpportunitiesData;
			}
			else
			{
				var lowerCaseFilter = filter.ToLower();

				ViewableOpportunitiesData = AllOpportunitiesData.Where(x =>
	               (x?.Company?.ToLower().Contains(lowerCaseFilter) ?? false )||
	               (x?.DateCreated.ToString().ToLower()?.Contains(lowerCaseFilter) ?? false) ||
	               (x?.DBA?.ToLower()?.Contains(lowerCaseFilter) ?? false) ||
	               (x?.LeaseAmountAsCurrency?.ToLower()?.Contains(lowerCaseFilter) ?? false) ||
	               (x?.Owner?.ToLower()?.Contains(lowerCaseFilter) ?? false) ||
	               (x?.SalesStage.ToString()?.ToLower()?.Contains(lowerCaseFilter) ?? false) ||
	               (x?.Topic?.ToLower()?.Contains(lowerCaseFilter) ?? false)
		 		).ToList();
			}
		}

		async Task InitializeDataInDatabaseAsync(int numberOfOpportunityModelsToGenerate = 20)
		{
			for (int i = 0; i < numberOfOpportunityModelsToGenerate; i++)
			{
				var tempModel = new OpportunityModel();

				var rnd = new Random();
				var companyIndex = rnd.Next(50);
				var dbaIndex = rnd.Next(50);
				var leaseAmount = rnd.Next(1000000);
				var ownerIndex = rnd.Next(50);
				var dayIndex = rnd.Next(1, 28);
				var monthIndex = rnd.Next(1, 12);
				var yearIndex = rnd.Next(2000, 2015);

				var salesStageNumber = rnd.Next(2);
				SalesStages salesStage;
				switch (salesStageNumber)
				{
					case 0:
						salesStage = SalesStages.New;
						break;
					case 1:
						salesStage = SalesStages.Pending;
						break;
					default:
						salesStage = SalesStages.Closed;
						break;
				}

				tempModel.Topic = $"{i + 715003} / Investment Data Corp";
				tempModel.Company = $"{LoremIpsumConstants.LoremIpsum.Substring(companyIndex, 10)}";
				tempModel.DBA = $"{LoremIpsumConstants.LoremIpsum.Substring(dbaIndex, 10)}";
				tempModel.LeaseAmount = leaseAmount;
				tempModel.SalesStage = salesStage;
				tempModel.Owner = $"{LoremIpsumConstants.LoremIpsum.Substring(ownerIndex, 10)}";
				tempModel.DateCreated = new DateTime(yearIndex, monthIndex, dayIndex);

				await App.Database.SaveOpportunityAsync(tempModel);
			}
		}

		async Task ExecuteRefreshAllDataCommand(bool isPullToRefreshCommanded)
		{
			await RefreshOpportunitiesDataAsync();

			if (isPullToRefreshCommanded)
				OnPullToRefreshDataCompleted();
		}

		void ExecuteFilterTextEnteredCommand(string filterText)
		{
			FilterList(filterText);
		}

		void ExecuteOkButtonTapped()
		{
			var handler = OkButtonTappedEvent;
			handler?.Invoke(null, EventArgs.Empty);

			Settings.ShouldShowWelcomeView = false;
		}

		void OnPullToRefreshDataCompleted()
		{
			var handler = PullToRefreshDataCompleted;
			handler?.Invoke(null, EventArgs.Empty);
		}
		#endregion
	}
}

