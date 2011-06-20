﻿// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.Jobs;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using PetaPoco;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTests : TestBase
    {
        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_new_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 1;
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew()
                .With(c => c.SeriesId = fakeSeries.SeriesId)
                .With(c => c.SeasonNumber = seasonNumber)
                .Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            var database = mocker.GetMock<IDatabase>(MockBehavior.Strict);
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1).Verifiable();


            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Once());
            mocker.VerifyAllMocks();

            result.SeasonNumber.Should().Be(fakeEpisode.SeasonNumber);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.DVD, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }

        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_new_daily_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"2011.01.10 - Denis Leary - HD TV.mkv";
            var airDate = new DateTime(2011, 01, 10);
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            var database = mocker.GetMock<IDatabase>(MockBehavior.Strict);
            database.Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();
            database.Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(1).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, airDate)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNotNull(result);
            mocker.GetMock<IDatabase>().VerifyAll();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Once());
            mocker.GetMock<EpisodeProvider>().VerifyAll();
            mocker.GetMock<DiskProvider>().VerifyAll();

            //Currently can't verify this since the list of episodes are loaded
            //Dynamically by SubSonic
            //Assert.AreEqual(fakeEpisode, result.EpisodeNumbers[0]);

            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.HDTV, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }

        [Test]
        public void import_existing_season_file_should_skip()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(true).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeNull();
        }

        [Test]
        [Description("Verifies that a un-parsable file isn't imported")]
        public void import_unparsable_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.avi";
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            Assert.IsNull(result);
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_sample_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"2011.01.10 - Denis Leary - sample - HD TV.mkv";
            var airDate = new DateTime(2011, 01, 10);
            const int size = 12345;
            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var mocker = new AutoMoqer();

            mocker.GetMock<IDatabase>()
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>())).Returns(false).Verifiable();
            mocker.GetMock<IDatabase>()
                .Setup(r => r.Insert(It.IsAny<EpisodeFile>())).Returns(0).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, airDate)).Returns(fakeEpisode).
                Verifiable();

            mocker.GetMock<DiskProvider>()
                .Setup(e => e.GetSize(fileName)).Returns(size).Verifiable();


            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void import_existing_file()
        {
            const string fileName = "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            var fakeSeries = Builder<Series>.CreateNew().Build();

            var mocker = new AutoMoqer();
            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(true).Verifiable();

            mocker.GetMock<EpisodeProvider>(MockBehavior.Strict);

            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            result.Should().BeNull();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Never());
            mocker.VerifyAllMocks();
        }

        [Test]
        [Description("Verifies that a file with no episode is skipped")]
        public void import_file_with_no_episode()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = "WEEDS.S03E01.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 01;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var mocker = new AutoMoqer();
            mocker.GetMock<IDatabase>(MockBehavior.Strict)
                .Setup(r => r.Exists<EpisodeFile>(It.IsAny<string>(), It.IsAny<object>())).Returns(false).Verifiable();

            mocker.GetMock<EpisodeProvider>(MockBehavior.Strict)
                .Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns<Episode>(null).
                Verifiable();

            mocker.GetMock<DiskProvider>(MockBehavior.Strict)
                .Setup(e => e.GetSize(fileName)).Returns(90000000000);


            //Act
            var result = mocker.Resolve<DiskScanProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            mocker.VerifyAllMocks();
            result.Should().BeNull();
            mocker.GetMock<IDatabase>().Verify(r => r.Insert(result), Times.Never());
            ExceptionVerification.ExcpectedWarns(1);
        }

        [Test]
        public void scan_series_should_update_last_scan_date()
        {

            var mocker = new AutoMoqer();
            mocker.GetMock<SeriesProvider>()
                .Setup(c => c.UpdateSeries(It.Is<Series>(s => s.LastDiskSync != null))).Verifiable();

            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(It.IsAny<long>()))
                .Returns(new List<Episode> { new Episode() });

            mocker.Resolve<DiskScanProvider>().Scan(new Series());

            mocker.VerifyAllMocks();

        }


        [Test]
        public void get_series_files()
        {
            var firstSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll().Have(s => s.SeriesId = 12).Build();

            var secondSeriesFiles = Builder<EpisodeFile>.CreateListOfSize(10)
                .WhereAll().Have(s => s.SeriesId = 20).Build();

            var mocker = new AutoMoqer();

            var database = MockLib.GetEmptyDatabase(true);

            foreach (var file in firstSeriesFiles)
                database.Insert(file);

            foreach (var file in secondSeriesFiles)
                database.Insert(file);

            mocker.SetConstant(database);

            var result = mocker.Resolve<MediaFileProvider>().GetSeriesFiles(12);


            result.Should().HaveSameCount(firstSeriesFiles);
        }

        [Test]
        public void Scan_series_should_skip_series_with_no_episodes()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(12))
                .Returns(new List<Episode>());

            var series = Builder<Series>.CreateNew()
                .With(s => s.SeriesId = 12).Build();

            //Act
            mocker.Resolve<DiskScanProvider>().Scan(series);

            //Assert
            mocker.VerifyAllMocks();

        }

        [Test]
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV]", "Law & Order- Criminal Intent - S10E07 - Icarus [HDTV]")]
        public void CleanFileName(string name, string expectedName)
        {
            //Act
            var result = MediaFileProvider.CleanFilename(name);

            //Assert
            Assert.AreEqual(expectedName, result);
        }
    }
}