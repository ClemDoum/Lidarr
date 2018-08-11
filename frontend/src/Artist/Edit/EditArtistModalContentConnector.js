import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import { setArtistValue, saveArtist } from 'Store/Actions/artistActions';
import EditArtistModalContent from './EditArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist,
    (state) => state.settings.languageProfiles,
    (state) => state.settings.metadataProfiles,
    createArtistSelector(),
    (artistState, languageProfiles, metadataProfiles, artist) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = artistState;

      const artistSettings = _.pick(artist, [
        'monitored',
        'qualityProfileId',
        'languageProfileId',
        'metadataProfileId',
        'path',
        'tags'
      ]);

      const settings = selectSettings(artistSettings, pendingChanges, saveError);

      return {
        artistName: artist.artistName,
        isSaving,
        saveError,
        isPathChanging: pendingChanges.hasOwnProperty('path'),
        originalPath: artist.path,
        item: settings.settings,
        showLanguageProfile: languageProfiles.items.length > 1,
        showMetadataProfile: metadataProfiles.items.length > 1,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetArtistValue: setArtistValue,
  dispatchSaveArtist: saveArtist
};

class EditArtistModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetArtistValue({ name, value });
  }

  onSavePress = (moveFiles) => {
    this.props.dispatchSaveArtist({
      id: this.props.artistId,
      moveFiles
    });
  }

  //
  // Render

  render() {
    return (
      <EditArtistModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        onMoveArtistPress={this.onMoveArtistPress}
      />
    );
  }
}

EditArtistModalContentConnector.propTypes = {
  artistId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetArtistValue: PropTypes.func.isRequired,
  dispatchSaveArtist: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditArtistModalContentConnector);
