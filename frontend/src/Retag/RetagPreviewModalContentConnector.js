import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { fetchRetagPreview } from 'Store/Actions/retagPreviewActions';
import { fetchMetadataProvider } from 'Store/Actions/settingsActions';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import RetagPreviewModalContent from './RetagPreviewModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.retagPreview,
    (state) => state.settings.metadataProvider,
    createArtistSelector(),
    (retagPreview, metadataSettings, artist) => {
      const props = { ...retagPreview };
      props.isFetching = retagPreview.isFetching || metadataSettings.isFetching;
      props.isPopulated = retagPreview.isPopulated && metadataSettings.isPopulated;
      props.error = retagPreview.error || metadataSettings.error;
      const writeAudioTags = metadataSettings.item.writeAudioTags;
      props.retagTracks = writeAudioTags === 'allFiles' || writeAudioTags === 'sync';
      props.path = artist.path;

      return props;
    }
  );
}

const mapDispatchToProps = {
  fetchRetagPreview,
  fetchMetadataProvider,
  executeCommand
};

class RetagPreviewModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchMetadataProvider();
  }

  componentDidUpdate(prevProps) {
    const {
      artistId,
      albumId,
      retagTracks,
      isPopulated,
      isFetching
    } = this.props;

    if (retagTracks && !isPopulated && !isFetching) {
      this.props.fetchRetagPreview({
        artistId,
        albumId
      });
    }
  }

  //
  // Listeners

  onRetagPress = (files) => {
    this.props.executeCommand({
      name: commandNames.RETAG_FILES,
      artistId: this.props.artistId,
      files
    });

    this.props.onModalClose();
  }

  //
  // Render

  render() {
    return (
      <RetagPreviewModalContent
        {...this.props}
        onRetagPress={this.onRetagPress}
      />
    );
  }
}

RetagPreviewModalContentConnector.propTypes = {
  artistId: PropTypes.number.isRequired,
  albumId: PropTypes.number,
  retagTracks: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  fetchRetagPreview: PropTypes.func.isRequired,
  fetchMetadataProvider: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RetagPreviewModalContentConnector);
