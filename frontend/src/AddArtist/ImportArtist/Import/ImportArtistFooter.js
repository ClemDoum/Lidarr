import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerButton from 'Components/Link/SpinnerButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import CheckInput from 'Components/Form/CheckInput';
import FormInputGroup from 'Components/Form/FormInputGroup';
import PageContentFooter from 'Components/Page/PageContentFooter';
import styles from './ImportArtistFooter.css';

const MIXED = 'mixed';

class ImportArtistFooter extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      defaultMetadataProfileId
    } = props;

    this.state = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      languageProfileId: defaultLanguageProfileId,
      metadataProfileId: defaultMetadataProfileId
    };
  }

  componentDidUpdate(prevProps, prevState) {
    const {
      defaultMonitor,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      defaultMetadataProfileId,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isLanguageProfileIdMixed,
      isMetadataProfileIdMixed
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      languageProfileId,
      metadataProfileId
    } = this.state;

    const newState = {};

    if (isMonitorMixed && monitor !== MIXED) {
      newState.monitor = MIXED;
    } else if (!isMonitorMixed && monitor !== defaultMonitor) {
      newState.monitor = defaultMonitor;
    }

    if (isQualityProfileIdMixed && qualityProfileId !== MIXED) {
      newState.qualityProfileId = MIXED;
    } else if (!isQualityProfileIdMixed && qualityProfileId !== defaultQualityProfileId) {
      newState.qualityProfileId = defaultQualityProfileId;
    }

    if (isLanguageProfileIdMixed && languageProfileId !== MIXED) {
      newState.languageProfileId = MIXED;
    } else if (!isLanguageProfileIdMixed && languageProfileId !== defaultLanguageProfileId) {
      newState.languageProfileId = defaultLanguageProfileId;
    }

    if (isMetadataProfileIdMixed && metadataProfileId !== MIXED) {
      newState.metadataProfileId = MIXED;
    } else if (!isMetadataProfileIdMixed && metadataProfileId !== defaultMetadataProfileId) {
      newState.metadataProfileId = defaultMetadataProfileId;
    }

    if (!_.isEmpty(newState)) {
      this.setState(newState);
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
    this.props.onInputChange({ name, value });
  }

  //
  // Render

  render() {
    const {
      selectedCount,
      isImporting,
      isLookingUpArtist,
      isMonitorMixed,
      isQualityProfileIdMixed,
      isLanguageProfileIdMixed,
      isMetadataProfileIdMixed,
      showLanguageProfile,
      showMetadataProfile,
      onImportPress,
      onCancelLookupPress
    } = this.props;

    const {
      monitor,
      qualityProfileId,
      languageProfileId,
      metadataProfileId
    } = this.state;

    return (
      <PageContentFooter>
        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Monitor
          </div>

          <FormInputGroup
            type={inputTypes.MONITOR_ALBUMS_SELECT}
            name="monitor"
            value={monitor}
            isDisabled={!selectedCount}
            includeMixed={isMonitorMixed}
            onChange={this.onInputChange}
          />
        </div>

        <div className={styles.inputContainer}>
          <div className={styles.label}>
            Quality Profile
          </div>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            value={qualityProfileId}
            isDisabled={!selectedCount}
            includeMixed={isQualityProfileIdMixed}
            onChange={this.onInputChange}
          />
        </div>

        {
          showLanguageProfile &&
            <div className={styles.inputContainer}>
              <div className={styles.label}>
                Language Profile
              </div>

              <FormInputGroup
                type={inputTypes.LANGUAGE_PROFILE_SELECT}
                name="languageProfileId"
                value={languageProfileId}
                isDisabled={!selectedCount}
                includeMixed={isLanguageProfileIdMixed}
                onChange={this.onInputChange}
              />
            </div>
        }

        {
          showMetadataProfile &&
            <div className={styles.inputContainer}>
              <div className={styles.label}>
                Metadata Profile
              </div>

              <FormInputGroup
                type={inputTypes.METADATA_PROFILE_SELECT}
                name="metadataProfileId"
                value={metadataProfileId}
                isDisabled={!selectedCount}
                includeMixed={isMetadataProfileIdMixed}
                onChange={this.onInputChange}
              />
            </div>
        }

        <div>
          <div className={styles.label}>
            &nbsp;
          </div>

          <div className={styles.importButtonContainer}>
            <SpinnerButton
              className={styles.importButton}
              kind={kinds.PRIMARY}
              isSpinning={isImporting}
              isDisabled={!selectedCount || isLookingUpArtist}
              onPress={onImportPress}
            >
              Import {selectedCount} Artist(s)
            </SpinnerButton>

            {
              isLookingUpArtist &&
                <Button
                  className={styles.loadingButton}
                  kind={kinds.WARNING}
                  onPress={onCancelLookupPress}
                >
                  Cancel Processing
                </Button>
            }

            {
              isLookingUpArtist &&
                <LoadingIndicator
                  className={styles.loading}
                  size={24}
                />
            }

            {
              isLookingUpArtist &&
                'Processing Folders'
            }
          </div>
        </div>
      </PageContentFooter>
    );
  }
}

ImportArtistFooter.propTypes = {
  selectedCount: PropTypes.number.isRequired,
  isImporting: PropTypes.bool.isRequired,
  isLookingUpArtist: PropTypes.bool.isRequired,
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  defaultLanguageProfileId: PropTypes.number,
  defaultMetadataProfileId: PropTypes.number,
  isMonitorMixed: PropTypes.bool.isRequired,
  isQualityProfileIdMixed: PropTypes.bool.isRequired,
  isLanguageProfileIdMixed: PropTypes.bool.isRequired,
  isMetadataProfileIdMixed: PropTypes.bool.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onImportPress: PropTypes.func.isRequired,
  onCancelLookupPress: PropTypes.func.isRequired
};

export default ImportArtistFooter;
