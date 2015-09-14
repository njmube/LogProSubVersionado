﻿using System;
using System.Collections.Generic;
using Core.BusinessEntity;
using Core.Validation;
using WpfFront.Common;
using WpfFront.WMSBusinessService;
using WpfFront.Presenters;

namespace WpfFront.Models
{
    public interface ITrackOptionModel
    {
        IList<ProductTrackRelation> TrackData { get; set; }
        IList<Label> ManualTrackList { get; set; }
        Int32 RemainingQty { get; set; }
        Unit TrackUnit { get; set; }
        Int32 MaxTrackQty { get; set; } //Maxim Quantity to Enter when tracking options activate, if serial number must be 1
        Label CurLabel { get; set; }
        IList<Label> UniqueTrackList { get; set; }

        //Vienen del control padre
        Product Product { get; set; }
        Document Document { get; set; }
        Node Node { get; set; }
        Unit CurUnit { get; set; }
        Bin Bin { get; set; }
        Int32 QtyPerPack { get; set; }
        Int32 QtyToTrack { get; set; }
        Int32 CurQtyPending { get; set; }

        Int16 TrackType { get; set; } //Indica 0=Receiving,1=Picking

    }

    public class TrackOptionModel : BusinessEntityBase, ITrackOptionModel
    {

        private IList<Label> _UniqueTrackList;
        public IList<Label> UniqueTrackList
        {
            get
            { return _UniqueTrackList; }
            set
            {
                _UniqueTrackList = value;
                OnPropertyChanged("UniqueTrackList");
            }
        }


        private Label _CurLabel;
        public Label CurLabel
        {
            get
            { return _CurLabel; }
            set
            {
                _CurLabel = value;
                OnPropertyChanged("CurLabel");
            }
        }



        private Unit trackunit;
        public Unit TrackUnit
        {
            get
            { return trackunit; }
            set
            {
                trackunit = value;
                OnPropertyChanged("TrackUnit");
            }
        }


        private Int32 maxtrackqty;
        public Int32 MaxTrackQty
        {
            get
            { return maxtrackqty; }
            set
            {
                maxtrackqty = value;
                OnPropertyChanged("MaxTrackQty");
            }
        }


        private Int32 remqty;
        public Int32 RemainingQty
        {
            get
            { return remqty; }
            set
            {
                remqty = value;
                OnPropertyChanged("RemainingQty");
            }
        }


        private Int32 curqtypending;
        public Int32 CurQtyPending
        {
            get
            { return curqtypending; }
            set
            {
                curqtypending = value;
                OnPropertyChanged("CurQtyPending");
            }
        }


        private IList<ProductTrackRelation> trackdata;
        public IList<ProductTrackRelation> TrackData
        {
            get
            { return trackdata; }
            set
            {
                trackdata = value;
                OnPropertyChanged("TrackData");
            }
        }


        private IList<Label> manualtracklist;
        public IList<Label> ManualTrackList
        {
            get
            { return manualtracklist; }
            set
            {
                manualtracklist = value;
                OnPropertyChanged("ManualTrackList");
            }
        }




        ///////////////////////////////////////////////////////////


        private Product product;
        public Product Product
        {
            get
            {
                return product;
            }
            set
            {
                product = value;
                OnPropertyChanged("Product");
            }
        }



        private Document document;
        public Document Document
        {
            get
            {
                return document;
            }
            set
            {
                document = value;
                OnPropertyChanged("Document");
            }
        }

        private Node node;
        public Node Node
        {
            get
            {
                return node;
            }
            set
            {
                node = value;
                OnPropertyChanged("Node");
            }
        }

        private Bin bin;
        public Bin Bin
        {
            get
            { return bin; }
            set
            {
                bin = value;
                OnPropertyChanged("Bin");
            }
        }


        private Int32 _QtyPerPack;
        public Int32 QtyPerPack
        {
            get
            { return _QtyPerPack; }
            set
            {
                _QtyPerPack = value;
                OnPropertyChanged("QtyPerPack");
            }
        }


        private Int32 _QtyToTrack;
        public Int32 QtyToTrack
        {
            get
            { return _QtyToTrack; }
            set
            {
                _QtyToTrack = value;
                OnPropertyChanged("QtyToTrack");
            }
        }


        private Unit curunit;
        public Unit CurUnit
        {
            get
            { return curunit; }
            set
            {
                curunit = value;
                OnPropertyChanged("CurUnit");
            }
        }


        private Int16 _TrackType;
        public Int16 TrackType
        {
            get
            { return _TrackType; }
            set
            {
                _TrackType = value;
                OnPropertyChanged("TrackType");
            }
        }

    }
}
