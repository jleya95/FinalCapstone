<template>
    <div v-if="canSee" class="CollectionOptions-popup">
        <div class="CollectionOptions-popup-inner">
            <button class="popup-exit-button" @click="showCollectionOptionsToParent">X</button>
            <div class="CollectionOptions-popup-inner-inner" :class="optionsToggle != 0 ? 'CollectionOptions-bigBox' : ''">
                <div class="CollectionOptions-buttons" style="margin-left: 8px;">
                    <div>
                        <button class="CollectionOptions-popup-inner-inner-buttons" style="background-color: #09A3DA;"
                            @click="optionsToggle = 1">Change Collection Name</button>
                    </div>
                    <div>
                        <button class="CollectionOptions-popup-inner-inner-buttons" style="background-color: #09A3DA;"
                            @click="optionsToggle = 2">Change Collection Privacy</button>
                    </div> 
                    <div>
                        <button style="width: 150px;" @click="areYouSurePopup = true">Delete Collection</button>
                    </div>
                </div>
                <div style="flex-grow: 2; align-items: stretch; margin: 12px;">
                    <div :class="optionsToggle == 1 ? 'makeVisible' : 'notVisible'" style="border-radius: 4px; ">
                        <div style="display: flex; flex-direction: column; margin-top: 4px;">
                        <p>What's the new name for your collection?</p>
                        <input type="text" id="newCollectionName" name="newCollectionName" placeholder="New name" 
                                v-model="updatedCollection.name"
                                style="align-items: center; margin: 12px; border-radius: 4px; width: 100%; color: white; background-color: black;">
                            <button style="background-color: #17B39F; width: 100%; margin: 10px;"
                                @click="changeCollectionName">Submit</button>
                            </div>
                    </div>
                    <div :class="optionsToggle == 2 ? 'makeVisible' : 'notVisible'" style="border-radius: 4px;">
                        <div style="display: flex; flex-direction: column; margin-top: 4px;">
                            <p>Change this collection's privacy setting</p>
                            <input type="checkbox" id="makePrivate" name="makePrivate" v-model="updatedCollection.is_private"
                                style="align-items: left; margin: 12px; border-radius: 4px; display: inline; color: white;">
                            <button style="margin: 12px; background-color: #17B39F;" @click="updatePrivacy">Update</button>
                        </div>
                    </div>
                    
                    <p v-if="weAreOnIt">We're on it!</p>
                    <p v-if="actionSuccessful">Success! Refreshing the page...</p>
                    <button v-if="optionsToggle != 0" @click="optionsToggle = 0">Close</button>
                </div>
            </div>
        </div>
    </div>


    <div v-if="errorPopup">
        <errorPopup v-bind:errorMessage="this.errorMessage" @click="errorPopup = !errorPopup"></errorPopup>
    </div>

    <div v-if="areYouSurePopup" class="CollectionOptions-AreYouSure-popup">
        <div class="CollectionOptions-AreYouSure-popup-inner">
            <button class="popup-exit-button" @click.prevent="areYouSurePopup = false">X</button>
            <div class="CollectionOptions-AreYouSure-popup-inner-inner" style="background-color: ">
                <p style="margin: 20px; font-weight: bold; padding-left: 0;">Are you sure? This will delete the entire colleciton. Your records will still be in your library</p>
                <button class="CollectionOptions-AreYouSure-popup-inner-inner-buttons"
                    style="background-color: black; color: white; font-size: 14px; height: 30px; border-radius: 4px; margin: 12px; margin-top: 20px; border-color: white; width: 150px;"
                    @click="deleteCollection">Yes</button>
                <button class="CollectionOptions-AreYouSure-popup-inner-inner-buttons"
                    style="background-color: #09A3DA; color: white; font-size: 14px; height: 30px; border-radius: 4px; margin: 12px; margin-bottom: 20px; border-color: white; width: 150px;"
                    @click="areYouSurePopup = false">Cancel</button>
            </div>
        </div>
    </div>
 
</template>

<script>
import ErrorPopup from './ErrorPopup.vue';
import CollectionsService from '../../services/CollectionsService';
export default {
    data() {
        return {
            errorPopup: false,
            errorMessage: "",
            weAreOnIt: false,
            canSee: this.isVisible,
            optionsToggle: 0,
            areYouSurePopup: false,
            updatedCollection: {
                name: '',
                is_private: this.collection.is_Private
            },
            
        }
    }, 
    components: {
        ErrorPopup
    },
    props: {
        isVisible: {
            type: Boolean,
            required: true
        },
        collection: {
            type: Object,
            required: true
        }

    },
    methods: {
        successDisappearer() {
            this.actionSuccessful = false;
            setTimeout(this.$router.go(), 2000)
            return this.actionSuccessful;
        },
        displaySuccess() {
            this.actionSuccessful = true;
            setTimeout(this.successDisappearer, 2000)
        },
        showCollectionOptionsToParent(response) {
            this.canSee = false;
            this.$emit('showCollectionOptionsToParent', false)
        },
        createCollection() {
            this.weAreOnIt = true;
            CollectionsService.AddNewCollection(this.newCollectionToAdd)
                .then(response => {
                    this.weAreOnIt = false;
                    this.displaySuccess();
                })
                .catch(error => {
                    this.weAreOnIt = false;
                    this.errorMessage = `create ${this.newCollectionToAdd.name}`;
                    this.errorPopup = true;
                })
        },
        changeCollectionName(){
            this.weAreOnIt = true;
            CollectionsService.ChangeNameForNamedCollection(this.collection.name, this.updatedCollection)
                .then(response => {
                    this.weAreOnIt = false;
                    this.displaySuccess();
                })
                .catch(error => {
                    this.weAreOnIt = false;
                    this.errorMessage = `update ${this.collection.name}`;
                    this.errorPopup = true;
                })
        },
        updatePrivacy(){
            this.weAreOnIt = true;
            this.updatedCollection.name = this.collection.name;
            CollectionsService.ChangePrivacyForNamedCollection(this.collection.name, this.updatedCollection)
                .then(response => {
                    this.weAreOnIt = false;
                    this.displaySuccess();
                })
                .catch(error => {
                    this.weAreOnIt = false;
                    this.errorMessage = `change ${this.collection.name}'s privacy settings'`;
                    this.errorPopup = true;
                })
        }, 
        deleteCollection(){
            this.areYouSurePopup = false;
            this.canSee = false;
            CollectionsService.DeleteNamedCollection(this.collection.name)
                .then(response => {
                    this.isLoading = true;
                    this.$router.go()
                })
                .catch(error => {
                    this.isLoading = true;
                    this.errorMessage = "deleting this record in your library";
                    this.errorPopup = true;
                })
        }
    }

}
</script>


<style>
.CollectionOptions-popup {
    border-radius: 5px;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    z-index: 99;
    background-color: rgba(0, 0, 0, 0.75);
    display: flex;
    align-items: center;
    justify-content: center;
    word-wrap: break-word;
}

.CollectionOptions-popup-inner scoped {
    border-radius: 5px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: black;
    opacity: 1.00;
    padding: 10px;
    text-align: center;
    height: 100%;
    width: 100%;
}

.CollectionOptions-popup-inner-inner {
    border-radius: 5px;
    background: black;
    text-align: center;
    height: 220px;
    width: 180px;
    display: flex;
     align-items: top;
     justify-content: stretch;
     margin: 12px;
}
.CollectionOptions-bigBox {
     height: 220px;
     width: 530px;
 }

 .CollectionOptions-buttons {
     display: flex;
     flex-direction: column;
     justify-content: stretch;
 }
 .CollectionOptions-popup-inner-inner-buttons {
     background-color: #09A3DA;
     width: 150px;
 }

.CollectionOptions-popup-inner button {
    border-radius: 5px;
    margin-top: 16px;
    padding: 8px 12px;
    background-color: #EA5143;
    color: #fff;
    border: none;
    text-align: center;
    border-radius: 4px;
    cursor: pointer;
    float: right;
}

.CollectionOptions-popup-inner-inputs {
    border-radius: 5px;
    box-sizing: border-box;
    width: 100%;
    background-color: black;
    color: white;
}
/* 
.CollectionOptions-popup-button {
    grid-area: button;
    margin-top: 8px;
    padding: 8px;
    background-color: #EA5143;
    min-width: 100%;
    color: #fff;
    text-align: center;
    border-radius: 4px;
    cursor: pointer;
} */

.CollectionOptions-AreYouSure-popup {
     border-radius: 5px;
     position: fixed;
     top: 0;
     left: 0;
     right: 0;
     bottom: 0;
     z-index: 99;
     background-color: rgba(0, 0, 0, 0.75);
     display: flex;
     align-items: center;
     justify-content: center;
     word-wrap: break-word;
 }

 .CollectionOptions-AreYouSure-popup-inner scoped {
     border-radius: 5px;
     display: flex;
     flex-direction: column;
     align-items: center;
     justify-content: center;
     background: black;
     opacity: 1.00;
     padding: 10px;
     text-align: center;
     height: 100%;
     width: 100%;
 }

 .CollectionOptions-AreYouSure-popup-inner button {
     border-radius: 5px;
     margin-top: 16px;
     padding: 8px 12px;
     background-color: #EA5143;
     color: #fff;
     border: none;
     text-align: center;
     border-radius: 4px;
     cursor: pointer;
     float: right;
 }

 .CollectionOptions-AreYouSure-popup-inner-inner {
     border-radius: 5px;
     background: #EA5143;
     ;
     text-align: center;
     height: 200;
     width: 200px;
     display: flex;
     flex-direction: column;
     align-items: center;
     margin: 12px;
 }
</style>