// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function incomingRecordForm(
  educationOrganizationId,
  educationOrganizations,
  searchReleasingSchoolUrl
) {
  return () => {
    return {
      educationOrganizationId,
      educationOrganizations,
      releasingSchools: [],

      streetNumberName: '',
      city: '',
      stateAbbreviation:'',
      postalCode: '',

      isSearchingReleasingSchool: false,
      searchReleasingSchoolError: '',
      releasingSchoolNotFoundErrorMessage: 'No releasing school was found using the provided search criteria. Try different search criteria.',

      get selectedEducationOrganization() {
        return this.educationOrganizations.find(it => it.id === this.educationOrganizationId)
      },

      getReleasingSchoolAddressLabel(releasingSchool) {
        const {
          streetNumberName,
          city,
          stateAbbreviation,
          postalCode,
        } = releasingSchool.address;
        return [streetNumberName, city, stateAbbreviation, postalCode].join(', ')
      },

      async searchReleasingSchool() {
        try {
          this.searchReleasingSchoolError = ''

          this.isSearchingReleasingSchool = true

          const params = new URLSearchParams()
          params.append("streetNumberName", this.streetNumberName)
          params.append("city", this.city)
          params.append("stateAbbreviation", this.stateAbbreviation)
          params.append("postalCode", this.postalCode)

          const url = `${searchReleasingSchoolUrl}?${params.toString()}`

          const response = await fetch(url);

          if (!response.ok) {
            throw new Error(this.releasingSchoolNotFoundErrorMessage)
          }

          const { items } = await response.json()

          if (items.length === 0) {
            throw new Error(this.releasingSchoolNotFoundErrorMessage)
          }

          this.releasingSchools = items
        } catch (error) {
          this.searchReleasingSchoolError = error.message
          this.releasingSchools = []
        } finally {
          this.isSearchingReleasingSchool = false
        }
      }
    }
  }
}
